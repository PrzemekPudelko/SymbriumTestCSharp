using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace SymbriumTestCSharp
{
    class Program
    {
        static void Main(string[] args)
        {

            Datasource datasource = new Datasource();
            // Open the connection to the database
            if(!datasource.open())
            {
                Console.Write("Can't open datasource");
                return;
            }

            // Storing all the data in the database into a list of Measurement objects
            List<Measurement> measurements = datasource.queryMeasurements();
            if (measurements == null || !measurements.Any())
            {
                Console.WriteLine("No measurements");
                return;
            }

                // Method call on measurements to perform the proper calculations
                process(measurements);
                // Close the connection to the database once finished
                datasource.close();
        }

        public static void process(List<Measurement> measurements)
        {
            // Sort the list by test_uid
            measurements.Sort();

            // Headers for the data being determined
            string headers =  "Test_UID, Min Height, Min Height Location, Max Height, "+
                "Max Height Location, Mean Height, Height Range, Average Roughness, "+
                "Standard Deviation, Measurements Inside Filter, Measurements Outside Filter\n";

            // writing to the summary.csv file within SymbriumTestCSharp
            string path = @"..\..\summary.csv";
            File.WriteAllText(path, headers);

            Console.WriteLine("How many standard deviations to use as a filter (Default is 3)");
            string input = Console.ReadLine();
            int numFilter = 3;

            // If the input passed in console cannot be parsed into an int,
            // then numFilter remains 3 as a default
            try
            {
                numFilter = int.Parse(input);
            } catch (FormatException e)
            {
                Console.WriteLine("Error: Integer not entered as input. " + e.Message);
            } finally
            {
                Console.WriteLine("Filtering to " + numFilter + " standard deviations.");
            }

            // For-loop ranges from Tests 1 to 10
            // In the event that more tests where to be added, 
            // measurements[measurements.Count()-1].test_uid ensures
            // that i ranges from the first to last test
            // because measurements is already sorted
            int count = 0;
            for(int i = 1; i <= measurements[measurements.Count()-1].test_uid; i++)
            {
                List<Measurement> list = new List<Measurement>();

                // Check that we do not exceed the number of measurements in list
                // and that only measurements of the same test_uid are added to the list
                while(count != measurements.Count() && measurements[count].test_uid == i)
                {
                    list.Add(measurements[count]);
                    count++;
                }

                // ensure that only tests with 1000 measurements are used
                if(list.Count == 1000)
                {
                    // return the computed data to append to the summary.csv file
                    string data = calculateTest(list, i, numFilter);
                    File.AppendAllText(path, data);
                } else
                {
                    Console.WriteLine("Test " + i + " did not contain 1000 meaurements");
                }
            }    
        }

        // List of 1000 measurements, test_uid and number of Standard Deviations to filter
        // with are the parameters
        // Immediately determine minHeight and its location, maxHeight and its location, 
        // mean height and height range
        public static string calculateTest(List<Measurement> list, int test, int numFilter)
        {
            double minHeight = double.MaxValue;
            int minHeightLocation = 0;
            double maxHeight = double.MinValue;
            int maxHeightLocation = 0;
            double meanHeight = 0.0;
            double heightRange = 0.0;

            // Save all heights into an array 
            // to pass into calculateAverageRoughness and calculateStandardDeviation
            double[] heightArr = new double[list.Count()];

            int i = 0;
            foreach(Measurement m in list)
            {
                double currentHeight = m.height;
                heightArr[i] = currentHeight;
                i++;
                meanHeight += currentHeight;

                if(currentHeight > maxHeight)
                {
                    maxHeight = currentHeight;
                    maxHeightLocation = m.measurement_uid;
                }

                if(currentHeight < minHeight)
                {
                    minHeight = currentHeight;
                    minHeightLocation = m.measurement_uid;
                }
            }

            meanHeight /= list.Count();
            heightRange = maxHeight - minHeight;

            // Pass the array of heights and mean height into methods 
            // to calculate averageRoughness and standardDevation
            // These 2 methods could also be combined into a single method 
            //and return a double array of size 2, where Average Roughness = arr[0]
            // and Standard Deviation = arr[1]
            double averageRoughness = calculateAverageRoughness(heightArr, meanHeight);
            double standardDeviation = calculateStandardDeviation(heightArr, meanHeight);

            int countInsideFilter = 0;
            int countOutsideFilter = 0;

            // Setting the Upper and Lower Limits for the Heights
            double bottomFilter = meanHeight - (numFilter * standardDeviation);
            double topFilter = meanHeight + (numFilter * standardDeviation);
            foreach(Measurement m in list)
            {
                double currentHeight = m.height;

                // Check if current height is outside the filter range
                // Count instances of being outside the range, and inside the range
                // assuming that instances where currentHeight = bottomFilter or
                // currentHeight = topFilter are considered within the range
                if(currentHeight < bottomFilter || currentHeight > topFilter)
                {
                    countOutsideFilter++;
                } else
                {
                    countInsideFilter++;
                }
            }

            // Formatting the returned data string into how it would appear in a csv file
            string comma = ", ";
            string data = test + comma + minHeight + comma + minHeightLocation + comma + maxHeight +
                comma + maxHeightLocation + comma + meanHeight + comma + heightRange + comma +
                averageRoughness + comma + standardDeviation + comma + countInsideFilter + comma
                + countOutsideFilter + "\n";
            return data;
        }

        // Loop through all heights in the array, subtract each from the mean height,
        // sum the absolute values of these results of these subtractions together, 
        // then divide by the number of values in the array
        public static double calculateAverageRoughness(double[] arr, double meanHeight)
        {
            if(arr == null)
            {
                Console.WriteLine("No Measurements");
                return 0.0;
            }
            double sum = 0.0;
            for(int i = 0; i < arr.Length; i++) 
            {
                sum += Math.Abs(arr[i] - meanHeight);
            }
            return sum / arr.Length;
        }

        // Loop through all heights in the array, subtract each from the mean height,
        // square the result, sum all of these results together then divide by the
        // number of values in the array
        public static double calculateStandardDeviation(double[] arr, double meanHeight)
        {
            if(arr == null)
            {
                Console.WriteLine("No Measurements");
                return 0.0;
            }

            double sum = 0.0;
            for(int i = 0; i < arr.Length; i++)
            {
                double result = arr[i] - meanHeight;
                sum += (result * result);
            }

            return Math.Sqrt(sum / arr.Length);
        }
    }
}
