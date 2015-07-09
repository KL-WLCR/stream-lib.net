using System;
using UInt32 = StreamLib.Utils.UInt32;

// [1] K.Whang et al. A Linear-Time Probabilistic Counting Algorithm for Database Applications
// http://dblab.kaist.ac.kr/Publication/pdf/ACM90_TODS_v15n2.pdf

namespace StreamLib.Cardinality
{
    public class LinearCounter
    {
        // taken from Table II of [1]
        static readonly uint[] OnePercentErrorLength =
            {
                    5034, 5067, 5100, 5133, 5166, 5199, 5231, 5264, 5296,                    // 100 - 900
                    5329, 5647, 5957, 6260, 6556, 6847, 7132, 7412, 7688,                    // 1000 - 9000
                    7960, 10506, 12839, 15036, 17134, 19156, 21117, 23029, 24897,            // 10000 - 90000
                    26729, 43710, 59264, 73999, 88175, 101932, 115359, 128514, 141441,       // 100000 - 900000
                    154171, 274328, 386798, 494794, 599692, 702246, 802931, 902069, 999894,  // 1000000 - 9000000
                    1096582                                                                  // 10000000
            };

        /// <summary>
        /// Internal bitmap, hashed stream elements are mapped to bits in this array.
        /// Do not modify it, use it for serialization.
        /// </summary>
        public readonly byte[] Bitmap;

        // size of the bitmap in bits
        readonly uint _bitmapBits;

        /// <summary>Number of bits left unset in the Bitmap</summary>
        public uint UnsetBits { get; private set; }

        /// <summary>
        /// Create Linear Counter with Bitmap with <paramref name="size"/>
        /// </summary>
        /// <param name="size">Bitmap size</param>
        public LinearCounter(uint size)
        {
            Bitmap = new byte[size];
            _bitmapBits = 8*size;
            UnsetBits = _bitmapBits;
        }

        /// <summary>
        /// Create Linear Counter with <paramref name="bitmap"/>
        /// </summary>
        /// <param name="bitmap">bitmap with which counter will be initialised</param>
        public LinearCounter(byte[] bitmap)
        {
            Bitmap = bitmap;
            _bitmapBits = 8*(uint)bitmap.Length;
            UnsetBits = CalcUnsetBits();
        }

        uint CalcUnsetBits()
        {
            uint c = 0;
            foreach (byte b in Bitmap)
                c += UInt32.BitCount(b);
            return _bitmapBits - c;
        }

        /// <summary>
        /// Create LinearCounter with arbitrary standard error and maximum expected cardinality.
        /// This method is more compute intensive than <code>CreateWithOnePercentError()</code> as it is perform
        /// solving precision inequality in runtime. Therefore, <code>CreateWithOnePercentError()</code> should be
        /// used whenever possible.
        /// </summary>
        /// <param name="eps">standard error as a fraction (e.g. 0.01 for 1%)</param>
        /// <param name="maxCardinality">maximum expected cardinality</param>
        public static LinearCounter CreateWithError(double eps, uint maxCardinality)
        {
            if (eps >= 1 || eps <= 0)
                throw new ArgumentOutOfRangeException("eps", "Epsilon should be in (0, 1) range");
            if (maxCardinality == 0)
                throw new ArgumentOutOfRangeException("maxCardinality", "Cardinality should be positive");

            int sz = ComputeRequiredBitMaskLength(maxCardinality, eps);
            return new LinearCounter((uint)Math.Ceiling(sz / 8D));
        }

        /// <summary>
        /// Create LinearCounter which keeps estimates below 1% error on average and has
        /// a low likelihood of saturation (0.7%) for any stream with cardinality less than <paramref name="maxCardinality"/>
        /// </summary>
        public static LinearCounter CreateWithOnePercentError(uint maxCardinality)
        {
            if (maxCardinality == 0)
                throw new ArgumentOutOfRangeException("maxCardinality", "maxCardinality must be a positive");

            uint length;
            if (maxCardinality < 100)
                length = OnePercentErrorLength[0];
            else if (maxCardinality < 10000000)
            {
                uint logscale = (uint)Math.Log10(maxCardinality);
                uint scaleValue = (uint)Math.Pow(10, logscale);
                uint scaleIndex = maxCardinality / scaleValue;
                uint index = 9 * (logscale - 2) + (scaleIndex - 1);
                uint lowerBound = scaleValue * scaleIndex;
                length = LinearInterpolation(lowerBound, OnePercentErrorLength[index], lowerBound + scaleValue, OnePercentErrorLength[index + 1], maxCardinality);
            }
            else if (maxCardinality < 50000000)
                length = LinearInterpolation(10000000, 1096582, 50000000, 4584297, maxCardinality);
            else if (maxCardinality < 100000000)
                length = LinearInterpolation(50000000, 4584297, 100000000, 8571013, maxCardinality);
            else if (maxCardinality <= 120000000)
                length = LinearInterpolation(100000000, 8571013, 120000000, 10112529, maxCardinality);
            else
                length = maxCardinality / 12;

            uint size = (uint)Math.Ceiling(length / 8D);
            return new LinearCounter(size);
        }

        /// <summary>OfferHashed the value as a uint hash value</summary>
        /// <param name="hash">the hash of the item to offer to the estimator</param>
        /// <returns>false if the value returned by Cardinality() is unaffected by the appearance of hash in the stream</returns>
        public bool OfferHashed(uint hash)
        {
            int bitIndex = (int) (hash % _bitmapBits);
            int byteIndex = bitIndex / 8;
            byte b = Bitmap[byteIndex];
            byte updateMask = (byte) (1 << (bitIndex % 8));
            if ((b & updateMask) == 0)
            {
                Bitmap[byteIndex] = (byte)(b | updateMask);
                UnsetBits--;
                return true;
            }
            return false;
        }

        /// <summary>Gather the cardinality estimate from this estimator</summary>
        public ulong Cardinality()
        {
            var Vn = _bitmapBits / (double) UnsetBits;
            double cardinality = Math.Round(_bitmapBits * Math.Log(Vn));
            if (double.IsInfinity(cardinality))
                return ulong.MaxValue;
            return (ulong) cardinality;
        }

        /// <summary>Merges estimators to produce an estimator for their combined streams</summary>
        /// <param name="estimators">Counters to merge</param>
        /// <returns>merged estimator or null if no estimators were provided</returns>
        public static LinearCounter MergeAll(params LinearCounter[] estimators)
        {
            if (estimators == null)
                throw new ArgumentNullException("estimators");
            if (estimators.Length == 0)
                throw new ArgumentOutOfRangeException("estimators", "Estimators should not be empty");

            int size = estimators[0].Bitmap.Length;
            byte[] mergedBytes = new byte[size];

            foreach (var estimator in estimators)
            {
                if (estimator.Bitmap.Length != size)
                    throw new Exception(string.Format("Cannot merge estimators of different sizes (first estimator size {0}, other estimator size {1}", size, estimator.Bitmap.Length));

                for (int b = 0; b < size; b++)
                    mergedBytes[b] |= estimator.Bitmap[b];
            }

            return new LinearCounter(mergedBytes);
        }

        /// <summary>Return new estimator which is result of merge this estimator with others</summary>
        public LinearCounter MergeWith(params LinearCounter[] estimators)
        {
            if (estimators == null)
                throw new ArgumentNullException("estimators");

            var lcs = new LinearCounter[estimators.Length + 1];
            Array.Copy(estimators, lcs, estimators.Length);
            lcs[lcs.Length - 1] = this;
            return MergeAll(lcs);
        }

        /// <summary>Runs binary search to find minimum bit mask length that holds precision inequality.</summary>
        /// <param name="n">expected cardinality</param>
        /// <param name="eps">desired standard error</param>
        /// <returns>minimal required bit mask length</returns>
        static int ComputeRequiredBitMaskLength(double n, double eps)
        {
            int fromM = 1;
            int toM = 100000000;
            int m;
            double eq;
            do
            {
                m = (toM + fromM)/2;
                double t = n/m;
                eq = Math.Max(1.0 / Math.Pow(eps * t, 2), 5) * (Math.Exp(t) - t - 1);
                if (m > eq)
                    toM = m;
                else
                    fromM = m + 1;
            } while (toM > fromM);
            return m > eq ? m : m + 1;
        }

        static uint LinearInterpolation(uint x0, uint y0, uint x1, uint y1, uint x)
        {
            return (uint)Math.Ceiling(y0 + (x - x0) * (double)(y1 - y0) / (x1 - x0));
        }
    }
}