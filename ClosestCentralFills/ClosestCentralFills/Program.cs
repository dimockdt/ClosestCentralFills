using System;
using System.Collections.Generic;
using System.Linq;

namespace ClosestCentralFills
{
    internal class Program
    {
        /// <summary>
        /// The number of nodes in the coordinate system that will have central fill facilities in them.
        /// </summary>
        private const int NUMBER_OF_COORDINATES_WITH_CENTRAL_FILL_FACILITIES = 10;

        /// <summary>
        /// The number of closest facilities to find after the user provides a point.
        /// </summary>
        private const int NUMBER_OF_CLOSEST_FACILITIES = 3;

        /// <summary>
        /// The maximum number of central fill facilities at any given node.
        /// </summary>
        private const int MAX_FACILITIES_PER_NODE = 1;

        /// <summary>
        /// The lower X bound of the coordinate system.
        /// </summary>
        private const int LOWER_BOUND_X = -10;

        /// <summary>
        /// The upper X bound of the coordinate system.
        /// </summary>
        private const int UPPER_BOUND_X = 10;

        /// <summary>
        /// The lower Y bound of the coordinate system.
        /// </summary>
        private const int LOWER_BOUND_Y = -10;

        /// <summary>
        /// The upper Y bound of the coordinate system.
        /// </summary>
        private const int UPPER_BOUND_Y = 10;

        /// <summary>
        /// Write a nice little greeting to the user in the console.
        /// </summary>
        private static void WriteGreeting()
        {
            DateTime now = DateTime.Now;
            TimeSpan timeOfDay = now.TimeOfDay;
            string timeOfDayStr = (timeOfDay.Hours < 12) ? "morning" : (timeOfDay.Hours < 18) ? "afternoon" : "evening";
            Console.WriteLine("Good {0}! Let's figure out what your closest central fill facilities are...", timeOfDayStr);
            Console.WriteLine();
        }

        /// <summary>
        /// Prompt the user to provide a point (x,y).
        /// </summary>
        /// <returns>Returns the point the user entered.</returns>
        private static Point ReadCoordinates()
        {
            bool validCoordinates = false;
            string exitWord = "done";
            Point userPoint = null;
            Console.Write("Please input your coordinates. To exit the program, enter '{0}': ", exitWord);
            while (!validCoordinates)
            {
                try
                {
                    string responseLine = Console.ReadLine().Replace(" ", string.Empty).ToLower();

                    if (responseLine == exitWord)
                    {
                        Console.WriteLine("Exitting...");
                        break;
                    }
                    else
                    {
                        string[] response = responseLine.Split(',');
                        int x = int.Parse(response[0]);
                        int y = int.Parse(response[1]);
                        x = (x > UPPER_BOUND_X) ? UPPER_BOUND_X :
                            (x < LOWER_BOUND_X) ? LOWER_BOUND_X : x;
                        y = (y > UPPER_BOUND_Y) ? UPPER_BOUND_Y :
                            (y < LOWER_BOUND_Y) ? LOWER_BOUND_Y : y;
                        userPoint = new Point(x, y);
                        validCoordinates = true;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Invalid format. Enter an integer value for X and an integer value for Y, seperated by a comma (e.x. 3,-8)");
                }
            }
            Console.WriteLine();
            return userPoint;
        }

        /// <summary>
        /// Given a list of coordinate system nodes, print their ID, location, distance from the user coordinate, and their medication prices.
        /// </summary>
        /// <param name="coordinates">The coordinates that the user provided.</param>
        /// <param name="nodes">The list of coordinate system nodes to print.</param>
        static void PrintResults(Point coordinates, List<CoordinateSystemNode> nodes)
        {
            Console.WriteLine("The {0} closest central fill facilities to ({1},{2}):", nodes.Count, coordinates.X, coordinates.Y);

            // go through each occupied coordinate system node
            for (int i = 0; i < nodes.Count; i++)
            {
                // sort each facility's prices in ascending order
                for (int j = 0; j < nodes[i].Facilities.Count; j++)
                {
                    nodes[i].Facilities[j].Prices = nodes[i].Facilities[j].Prices.OrderBy(n => n.Value).ToList();
                    Console.WriteLine("Central Fill {0} - ${1}, Medication {2}, Distance {3}",
                        nodes[i].Facilities[j].ID, nodes[i].Facilities[j].Prices[0].Value.ToString("0.00"), nodes[i].Facilities[j].Prices[0].Key, nodes[i].DistanceToUserCoordinate);
                }
            }
            Console.WriteLine();
        }

        /// <summary>
        /// Main program entry point.
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // greet our user
            WriteGreeting();

            // create our coordinate system with its central fill facilities
            CoordinateSystem map = new CoordinateSystem(LOWER_BOUND_X, UPPER_BOUND_X, LOWER_BOUND_Y, UPPER_BOUND_Y, NUMBER_OF_COORDINATES_WITH_CENTRAL_FILL_FACILITIES);
            map.GenerateRandomPoints();
            map.PopulateNodeMap(MAX_FACILITIES_PER_NODE);

            Console.WriteLine();

            // repeat this process unntil the user tells us to exit
            Point userCoordinates;
            List<CoordinateSystemNode> closestStations;
            while (true)
            {
                userCoordinates = ReadCoordinates();
                if (userCoordinates == null)
                {
                    break;
                }
                else
                {
                    closestStations = map.ClosestNode(userCoordinates, NUMBER_OF_CLOSEST_FACILITIES);
                    PrintResults(userCoordinates, closestStations);
                }
            }
        }
    }

    /// <summary>
    /// Class that creates a model of our coordinate system world.
    /// </summary>
    public class CoordinateSystem
    {
        /// <summary>
        /// Our array of nodes, equal in size to the coordinate system. If there are no central fill facilities on a given node, the value at that index of the array will be null.
        /// </summary>
        private CoordinateSystemNode[,] m_NodeMap;

        /// <summary>
        /// List of coordinate system nodes that have central fill facilities in them. There are no null nodes in this list.
        /// </summary>
        private List<CoordinateSystemNode> m_OccupiedNodes;

        /// <summary>
        /// List of points (-x:+x, -y:+y) where the randomly-placed central fill facilities will be placed in the coordinate system.
        /// </summary>
        private List<Point> m_CentralFillPoints;

        /// <summary>
        /// The total number of nodes that will have a central fill facility in them.
        /// </summary>
        private int m_NumberOfCentralFillFacilities;

        /// <summary>
        /// The lower X bound of the coordinate system.
        /// </summary>
        private int m_LowerBoundX;

        /// <summary>
        /// The upper X bound of the coordinate system.
        /// </summary>
        private int m_UpperBoundX;

        /// <summary>
        /// The lower Y bound of the coordinate system.
        /// </summary>
        private int m_LowerBoundY;

        /// <summary>
        /// The upper Y bound of the coordinate system.
        /// </summary>
        private int m_UpperBoundY;

        /// <summary>
        /// Create an instance of the CoordinateSystem class.
        /// </summary>
        /// <param name="xLowerBound">The lower X bound of the coordinate system.</param>
        /// <param name="xUpperBound">The upper X bound of the coordinate system.</param>
        /// <param name="yLowerBound">The lower Y bound of the coordinate system.</param>
        /// <param name="yUpperBound">The upper Y bound of the coordinate system.</param>
        /// <param name="numberOfCentralFillFacilities">The total number of nodes that will have a central fill facility in them.</param>
        public CoordinateSystem(int xLowerBound, int xUpperBound, int yLowerBound, int yUpperBound, int numberOfCentralFillFacilities)
        {
            m_LowerBoundX = xLowerBound;
            m_UpperBoundX = xUpperBound;
            m_LowerBoundY = yLowerBound;
            m_UpperBoundY = yUpperBound;

            m_OccupiedNodes = new List<CoordinateSystemNode>();
            m_NodeMap = new CoordinateSystemNode[Math.Abs(m_LowerBoundX) + Math.Abs(m_UpperBoundX) + 1, Math.Abs(m_LowerBoundY) + Math.Abs(m_UpperBoundY) + 1]; // plus one because there's a 0,0 point, and our nodes are on the vertices of the coordinate system
            m_NumberOfCentralFillFacilities = numberOfCentralFillFacilities;
        }

        /// <summary>
        /// Using a random number generator, create a list of points inside the bounds of the coordinate system that will have central fill facilities in them.
        /// Must be called before populating the node map.
        /// </summary>
        public void GenerateRandomPoints()
        {
            m_CentralFillPoints = new List<Point>();

            int randomX, randomY;
            Point randomPoint;
            Random rng = new Random();

            // make a list of random points that are within the given bounds
            for (int i = 0; i < m_NumberOfCentralFillFacilities; i++)
            {
                randomX = rng.Next(m_LowerBoundX, m_UpperBoundX + 1);
                randomY = rng.Next(m_LowerBoundY, m_UpperBoundY + 1);
                randomPoint = new Point(randomX, randomY);

                // if this random point already exists in the list, try again
                if (!m_CentralFillPoints.Contains(randomPoint))
                {
                    m_CentralFillPoints.Add(randomPoint);
                }
                else
                {
                    i--;
                }
            }
        }

        /// <summary>
        /// Take the list of randomly generated points in the coordinate system and create a node with a central fill facility at each point.
        /// Must generate random points before calling.
        /// </summary>
        /// <param name="numberOfFills">The number of central fill facilities at each coordinate system node.</param>
        public void PopulateNodeMap(int numberOfFills)
        {
            // Create a coordinate system node with a central fill at each of the random points in the node map
            for (int i = 0; i < m_CentralFillPoints.Count; i++)
            {
                // translate the points from the +/-X, +/-Y space to non-negative indicies
                int xIndexOfPoint = m_CentralFillPoints[i].X + Math.Abs(m_LowerBoundX);
                int yIndexOfPoint = m_CentralFillPoints[i].Y + Math.Abs(m_LowerBoundY);

                // make sure the point exists in the node map so we don't get an index out-of-range exception
                if (xIndexOfPoint >= 0 && xIndexOfPoint < m_NodeMap.GetLength(0) &&
                    yIndexOfPoint >= 0 && yIndexOfPoint < m_NodeMap.GetLength(1))
                {
                    CoordinateSystemNode node = new CoordinateSystemNode(m_CentralFillPoints[i], numberOfFills);
                    m_NodeMap[xIndexOfPoint, yIndexOfPoint] = node;
                    m_OccupiedNodes.Add(node);
                }
            }
        }

        /// <summary>
        /// Print the generated node map to the console for the user to see.
        /// </summary>
        public void PrintNodeMap()
        {
            Console.WriteLine("Central Fills Stations:");
            foreach (Point point in m_CentralFillPoints)
            {
                Console.WriteLine("X: {0}\tY: {1}", point.X, point.Y);
            }

            Console.WriteLine();
            Console.WriteLine("Node Map:");
            for (int i = m_NodeMap.GetLength(1) - 1; i >= 0; i--)
            {
                for (int j = 0; j < m_NodeMap.GetLength(0); j++)
                {
                    if (m_NodeMap[j, i] != null)
                    {
                        Console.Write("$  ");
                    }
                    else
                    {
                        Console.Write(".  ");
                    }
                }
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Calculate the manhattan distance between the provided point and all the central fill facilities in the coordinate system. Sort the list in ascending order, then return a list of the closest nodes.
        /// </summary>
        /// <param name="point">The user-provided point to calculate the manhattan distances against.</param>
        /// <param name="numberOfPoints">The number of closest points to return. For example, if set to 3, function will return up to 3 of the closest nodes to the provided point.</param>
        /// <returns>Returns a list of size equal to the provided number of points of the closest nodes to the provided point.</returns>
        public List<CoordinateSystemNode> ClosestNode(Point point, int numberOfPoints)
        {
            List<CoordinateSystemNode> closestNodes = new List<CoordinateSystemNode>();

            // make sure this isn't null for some reason
            if (m_OccupiedNodes != null & m_OccupiedNodes.Count > 0)
            {
                // calculate the distance between each node with a central fill facility and the user-provided coordinate
                for (int i = 0; i < m_OccupiedNodes.Count; i++)
                {
                    m_OccupiedNodes[i].DistanceToUserCoordinate = ManhattanDistance(point, m_OccupiedNodes[i].Location);
                }

                // sort the list of occupied nodes in ascending order of distance to the provided point
                m_OccupiedNodes = m_OccupiedNodes.OrderBy(n => n.DistanceToUserCoordinate).ToList();

                // make sure that the number of points the user requested doesn't exceed the total number of occupied nodes
                numberOfPoints = (numberOfPoints > m_OccupiedNodes.Count) ? m_OccupiedNodes.Count : numberOfPoints;

                // take up to the first three nodes of the sorted list
                for (int i = 0; i < numberOfPoints; i++)
                {
                    closestNodes.Add(m_OccupiedNodes[i]);
                }
            }
            
            return closestNodes;
        }

        /// <summary>
        /// Calculate the Manhattan between two given points.
        /// </summary>
        /// <param name="pointA">The first point.</param>
        /// <param name="pointB">The second point.</param>
        /// <returns>Returns the manhattan distance between the two provided points.</returns>
        public int ManhattanDistance(Point pointA, Point pointB)
        {
            return (Math.Abs(pointA.X - pointB.X) + Math.Abs(pointA.Y - pointB.Y));
        }
    }

    /// <summary>
    /// Class that enumerates a point in the coordinate system.
    /// </summary>
    public class Point
    {
        /// <summary>
        /// The X location of the point.
        /// </summary>
        private int m_X;

        /// <summary>
        /// The Y location of the point.
        /// </summary>
        private int m_Y;

        /// <summary>
        /// The X location of the point.
        /// </summary>
        public int X
        {
            get { return m_X; }
            set { m_X = value; }
        }

        /// <summary>
        /// The Y location of the point.
        /// </summary>
        public int Y
        {
            get { return m_Y; }
            set { m_Y = value; }
        }

        /// <summary>
        /// Create an instance of the Point class.
        /// </summary>
        /// <param name="x">The X location of the point.</param>
        /// <param name="y">The Y location of the point.</param>
        public Point(int x, int y)
        {
            m_X = x;
            m_Y = y;
        }
    }

    /// <summary>
    /// Class that enumerates a node in the coordinate system. A node can be null, or carry a central fill facility.
    /// </summary>
    public class CoordinateSystemNode
    {
        /// <summary>
        /// This node's central fill facility.
        /// </summary>
        private List<CentralFillFacility> m_Facilities;

        /// <summary>
        /// The point in the coordinate system where this node is (x,y).
        /// </summary>
        private Point m_CoordinateSystemLocation;

        /// <summary>
        /// The manhattan distance between this node and the user-defined coordinate.
        /// </summary>
        private int m_DistanceToUserCoordinate;

        /// <summary>
        /// This node's central fill facility.
        /// </summary>
        public List<CentralFillFacility> Facilities
        {
            get { return m_Facilities; }
            set { m_Facilities = value; }
        }

        /// <summary>
        /// The point in the coordinate system where this node is (x,y).
        /// </summary>
        public Point Location
        {
            get { return m_CoordinateSystemLocation; }
            set { m_CoordinateSystemLocation = value; }
        }

        /// <summary>
        /// The manhattan distance between this node and the user-defined coordinate.
        /// </summary>
        public int DistanceToUserCoordinate
        {
            get { return m_DistanceToUserCoordinate; }
            set { m_DistanceToUserCoordinate = value; }
        }

        /// <summary>
        /// Create an instance of the CoordinateSystemNode class.
        /// </summary>
        /// <param name="coordinateSystemLocation">The point in the coordinate system that this node lies at.</param>
        /// <param name="numberOfFacilities">The number of central fill facilities at this coordinate system node.</param>
        public CoordinateSystemNode(Point coordinateSystemLocation, int numberOfFacilities)
        {
            m_Facilities = new List<CentralFillFacility>();
            m_CoordinateSystemLocation = coordinateSystemLocation;

            // create a number of central fill facilities equal to the number provided
            for (int i = 0; i < numberOfFacilities; i++)
            {
                CentralFillFacility facility = new CentralFillFacility();
                string message = string.Format("Central Fill Facility {0}, at ({1},{2}):\t", facility.ID, m_CoordinateSystemLocation.X, m_CoordinateSystemLocation.Y);
                foreach (KeyValuePair<MedicationType, double> price in facility.Prices)
                {
                    message += string.Format("Medication {0}: ${1}   ", price.Key, price.Value.ToString("0.00"));
                }
                m_Facilities.Add(facility);
                Console.WriteLine(message);
            }
        }
    }

    /// <summary>
    /// Class that enumerates a central fill facility.
    /// </summary>
    public class CentralFillFacility
    {
        /// <summary>
        /// The maximum price of a fill (USD).
        /// </summary>
        private const double MAX_PRICE = 200.0;

        /// <summary>
        /// The total number of instances of this class in this program. This is what we use to ID the central fill facilities.
        /// </summary>
        private static int InstanceCount = 0;

        /// <summary>
        /// The ID of this central fill facility.
        /// </summary>
        private string m_ID;

        /// <summary>
        /// List of the medications at this central fill facility and their prices.
        /// </summary>
        private List<KeyValuePair<MedicationType, double>> m_Prices;

        /// <summary>
        /// Random number generator used to assign prices to each medication.
        /// </summary>
        private static Random m_RNG = new Random();

        /// <summary>
        /// The ID of this central fill facility.
        /// </summary>
        public string ID
        {
            get { return m_ID; }
            set { m_ID = value; }
        }

        /// <summary>
        /// List of the medications at this central fill facility and their prices.
        /// </summary>
        public List<KeyValuePair<MedicationType, double>> Prices
        {
            get { return m_Prices; }
            set { m_Prices = value; }
        }

        /// <summary>
        /// Create an instance of the CentralFillFacility class.
        /// </summary>
        public CentralFillFacility()
        {
            InstanceCount++;
            m_ID = InstanceCount.ToString("000");
            m_Prices = new List<KeyValuePair<MedicationType, double>>();
            for (int i = 0; i < Enum.GetNames(typeof(MedicationType)).Length; i++)
            {
                m_Prices.Add(new KeyValuePair<MedicationType, double>((MedicationType)i, Math.Round(m_RNG.NextDouble() * MAX_PRICE, 2)));
            }
        }
    }

    /// <summary>
    /// The different medication types.
    /// </summary>
    public enum MedicationType
    {
        A,
        B,
        C
    }
}
