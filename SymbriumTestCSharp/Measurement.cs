using System;

namespace SymbriumTestCSharp
{
    class Measurement : IComparable<Measurement>
    {
        public int measurement_uid { get; }
        public int test_uid { get; }
        public float x { get; }
        public float y { get; }
        public double height { get; }

        // Constructor
        public Measurement(int measurement_uid, int test_uid, float x, float y, double height)
        {
            this.measurement_uid = measurement_uid;
            this.test_uid = test_uid;
            this.x = x;
            this.y = y;
            this.height = height;
        }

        // Measurement Implements Comparable
        // Compares the test_uids of the 2 measurements
        // so that later the Measurement objects may be sorted
        // by test_uid
        public int CompareTo(Measurement other)
        {
            return test_uid.CompareTo(other.test_uid);
        }
    }
}
