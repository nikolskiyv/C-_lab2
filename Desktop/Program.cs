using System;
using System.Numerics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Nikolskiy_lab2
{
    public struct Grid2D
    {
        public float step_x { get; set; }
        public int num_of_nodes_x { get; set; }
        public float step_y { get; set; }
        public int num_of_nodes_y { get; set; }

        public Grid2D(float x1 = 1, float y1 = 1, int x2 = 5, int y2 = 5)
        {
            step_x = x1;
            step_y = y1;
            num_of_nodes_x = x2;
            num_of_nodes_y = y2;
        }

        public override string ToString()
        {
            return "\nStep x: " + step_x.ToString() + " Num of nodes x: " + num_of_nodes_x.ToString() +
                " Step y: " +
                step_y.ToString() + " Num of nodes y: " + num_of_nodes_y.ToString();
        }

        public string ToString(string format)
        {
            return "\nStep x: " + step_x.ToString(format) + " Num of nodes x: " + num_of_nodes_x.ToString(format) +
                " Step y: " +
                step_y.ToString(format) + " Num of nodes y: " + num_of_nodes_y.ToString(format);
        }
    }

    abstract public class V5Data
    {
        public string info { get; set; }
        public DateTime date { get; set; }

        public V5Data(String x1, DateTime x2)
        {
            info = x1;
            date = x2;
        }

        public abstract Vector2[] NearEqual(float eps);
        public abstract string ToLongString();

        public override string ToString()
        {
            return info.ToString() + " " + date.ToString();
        }

        public virtual string ToString(string format)
        {
            return info.ToString() + " " + date.ToString(format);
        }
    }

    public class V5DataCollection : V5Data, IEnumerable<DataItem>
    {
        public Dictionary<System.Numerics.Vector2, System.Numerics.Vector2> elements { get; set; }

        public V5DataCollection(string x1, DateTime x2) : base(x1, x2)
        {
            elements = new Dictionary<Vector2, Vector2>();
        }

        public V5DataCollection(string x1, DateTime x2, string filename) : base(x1, x2)
        {
            /* Данные хранятся в виде четырех столбцов "x y x_data y_data", где x и y - координаты, а x_data и y_data - значения поля в точке
               Разделители - пробелы */
            try
            {
                elements = new Dictionary<Vector2, Vector2>();
                using (StreamReader sr = new StreamReader(filename))
                {
                    string line, cop;
                    Vector2 key, value;
                    float x, y, x_data, y_data;
                    while ((line = sr.ReadLine()) != null)
                    {
                        cop = line;

                        string[] words = cop.Split(new char[] { ' ' });

                        x = float.Parse(words[0]);
                        y = float.Parse(words[1]);
                        x_data = float.Parse(words[2]);
                        y_data = float.Parse(words[3]);

                        key = new Vector2(x, y);
                        value = new Vector2(x_data, y_data);
                        Console.WriteLine(value);
                        elements.Add(key, value);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }
        }

        public void InitRandom(int nItems, float xmax, float ymax, float minValue, float maxValue)
        {
            Random rand = new Random();
            float k1, k2, k3, k4, x, y, x_data, y_data;
            Vector2 key, value;
            for (int i = 0; i < nItems; i++)
            {
                k1 = (float)rand.NextDouble();
                k2 = (float)rand.NextDouble();
                k3 = (float)rand.NextDouble();
                k4 = (float)rand.NextDouble();
                x_data = minValue * k1 + maxValue * (1 - k1);
                y_data = minValue * k2 + maxValue * (1 - k2);
                x = xmax * k3;
                y = ymax * k4;
                key = new Vector2(x, y);
                value = new Vector2(x_data, y_data);
                elements.Add(key, value);
            }
        }

        public override Vector2[] NearEqual(float eps)
        {
            List<Vector2> list = new List<Vector2>();
            foreach (KeyValuePair<Vector2, Vector2> kvp in elements)
            {
                Vector2 theElement = kvp.Value;
                if (Math.Abs(theElement.X - theElement.Y) <= eps)
                    list.Add(theElement);

            }
            Vector2[] array = list.ToArray();
            return array;
        }

        public override string ToString()
        {
            string str = "V5DataCollection ";
            str += info + " " + date.ToString() + "\nNum of elements: " + elements.Count + "\n";
            return str;
        }

        public override string ToLongString()
        {
            string str = "V5DataCollection ";
            str += info + " " + date.ToString() + "\nNum of elements: " + elements.Count + "\n";
            foreach (KeyValuePair<Vector2, Vector2> kvp in elements)
            {
                str += kvp.Key + " " + kvp.Value + "\n";
            }
            return str;
        }

        public string ToLongString(string format)
        {
            string str = "V5DataCollection ";
            str += info + " " + date.ToString(format) + "\nNum of elements: " + elements.Count + "\n";
            foreach (KeyValuePair<Vector2, Vector2> kvp in elements)
            {
                str += kvp.Key + " " + kvp.Value.ToString(format) + "\n";
            }
            return str;
        }

        public IEnumerator<DataItem> GetEnumerator()
        {
            List<DataItem> list = new List<DataItem>();
            Vector2 val, coord;
            DataItem tmp;

            foreach (KeyValuePair<Vector2, Vector2> kvp in elements)
            {
                val = kvp.Value;
                coord = kvp.Key;
                tmp = new DataItem(coord, val);
                list.Add(tmp);
            }
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            List<DataItem> list = new List<DataItem>();
            Vector2 val, coord;
            DataItem tmp;

            foreach (KeyValuePair<Vector2, Vector2> kvp in elements)
            {
                val = kvp.Value;
                coord = kvp.Key;
                tmp = new DataItem(coord, val);
                list.Add(tmp);
            }
            return list.GetEnumerator();
        }
    }

    public class V5DataOnGrid : V5Data, IEnumerable<DataItem>
    {
        public Grid2D grid { get; set; }
        public Vector2[,] value_in_nodes { get; set; }

        public V5DataOnGrid(string x1, DateTime x2, Grid2D x3) : base(x1, x2)
        {
            grid = x3;
            value_in_nodes = new Vector2[grid.num_of_nodes_x, grid.num_of_nodes_y];
        }
        public void InitRandom(float minValue, float maxValue)
        {
            Random rand = new Random();
            float k1, k2, x, y;
            for (int i = 0; i < grid.num_of_nodes_x; i++)
                for (int j = 0; j < grid.num_of_nodes_y; j++)
                {
                    k1 = (float)rand.NextDouble();
                    k2 = (float)rand.NextDouble();
                    x = minValue * k1 + maxValue * (1 - k1);
                    y = minValue * k2 + maxValue * (1 - k2);
                    value_in_nodes[i, j] = new Vector2(x, y);
                }
        }

        public override Vector2[] NearEqual(float eps)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 0; i < grid.num_of_nodes_x; i++)
                for (int j = 0; j < grid.num_of_nodes_y; j++)
                    if (Math.Abs(value_in_nodes[i, j].X - value_in_nodes[i, j].Y) <= eps)
                        list.Add(value_in_nodes[i, j]);
            Vector2[] array = list.ToArray();
            return array;
        }

        public static explicit operator V5DataCollection(V5DataOnGrid x)
        {
            int i, j;
            Vector2 key, value;
            V5DataCollection Result;
            Result = new V5DataCollection(x.info, x.date);
            for (i = 0; i < x.grid.num_of_nodes_x; i++)
                for (j = 0; j < x.grid.num_of_nodes_y; j++)
                {
                    key = new Vector2(i, j);
                    value = new Vector2(x.value_in_nodes[i, j].X, x.value_in_nodes[i, j].Y);
                    Result.elements.Add(key, value);
                }
            return Result;
        }

        public override string ToString()
        {
            string str = "V5DataOnGrid ";
            str += info + " " + date.ToString() + " " + grid.ToString() + "\n";
            return str;
        }

        public override string ToLongString()
        {
            string str = "V5DataOnGrid ";
            str += info + " " + date.ToString() + " " + grid.ToString() + "\n";
            for (int i = 0; i < grid.num_of_nodes_x; i++)
                for (int j = 0; j < grid.num_of_nodes_y; j++)
                {
                    str += "[" + i + "," + j + "] " + "(" + value_in_nodes[i, j].X + "," + value_in_nodes[i, j].Y + ")\n";
                }

            return str;
        }

        public string ToLongString(string format)
        {
            string str = "V5DataOnGrid ";
            str += info + " " + date.ToString(format) + " " + grid.ToString(format) + "\n";
            for (int i = 0; i < grid.num_of_nodes_x; i++)
                for (int j = 0; j < grid.num_of_nodes_y; j++)
                {
                    str += "[" + i + "," + j + "] " + "(" + value_in_nodes[i, j].X.ToString(format) + "," + value_in_nodes[i, j].Y.ToString(format) + ")\n";
                }

            return str;
        }

        public IEnumerator<DataItem> GetEnumerator()
        {
            List<DataItem> list = new List<DataItem>();
            DataItem tmp;
            Vector2 coordinate, value;
            for (int i = 0; i < grid.num_of_nodes_x; i++)
                for (int j = 0; j < grid.num_of_nodes_y; j++)
                {
                    coordinate.X = i;
                    coordinate.Y = j;
                    value.X = value_in_nodes[i, j].X;
                    value.Y = value_in_nodes[i, j].Y;
                    tmp = new DataItem(coordinate, value);
                    list.Add(tmp);
                }
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            List<DataItem> list = new List<DataItem>();
            DataItem tmp;
            Vector2 coordinate, value;
            for (int i = 0; i < grid.num_of_nodes_x; i++)
                for (int j = 0; j < grid.num_of_nodes_y; j++)
                {
                    coordinate.X = i;
                    coordinate.Y = j;
                    value.X = value_in_nodes[i, j].X;
                    value.Y = value_in_nodes[i, j].Y;
                    tmp = new DataItem(coordinate, value);
                    list.Add(tmp);
                }
            return list.GetEnumerator();
        }

    }

    class V5MainCollection : IEnumerable
    {
        private List<V5Data> list;
        IEnumerable<V5Data> Data;

        float x1 = 1, y1 = 1;
        int x2 = 5, y2 = 5;

        Grid2D item;

        public IEnumerator GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int Count()
        {
            return list.Count;
        }

        public void Add(V5Data item)
        {
            list.Add(item);
        }

        public bool Remove(string id, DateTime date)
        {
            bool f = false;
            for (int i = 0; i < list.Count; i++)
            {
                if (String.Equals(list[i].info, id) == true && list[i].date.CompareTo(date) == 0)
                {
                    list.RemoveAt(i);
                    i--;
                    f = true;
                }
            }
            return f;
        }

        public void AddDefaults()
        {
            Random rnd = new Random();
            int NumOfElements = rnd.Next(3, 5), n;
            Grid2D item;
            V5DataCollection obj1;
            V5DataOnGrid obj2;
            int bin;
            list = new List<V5Data>();
            for (int i = 0; i < NumOfElements; i++)
            {
                bin = rnd.Next(0, 2);
                item = new Grid2D(1, 1, 2, 2);
                if (bin == 0)
                {
                    obj2 = new V5DataOnGrid("", DateTime.Now, item);
                    obj2.InitRandom(1, 4);
                    list.Add(obj2);
                }
                else
                {
                    n = rnd.Next(1, 20);
                    obj1 = new V5DataCollection("", DateTime.Now);
                    obj1.InitRandom(n, 4, 5, 1, 4);
                    list.Add(obj1);
                }
            }
        }

        public override string ToString()
        {
            string str = "";
            foreach (V5Data item in list)
            {
                str += item.ToString();
            }
            return str;
        }

        public string ToLongString(string format)
        {
            string str = "";
            foreach (V5Data item in list)
            {
                str += item.ToString(format);
            }
            return str;
        }

        /*public static float DistanceBetweenTwoPoints(Vector2 point1, Vector2 point2)
        {
            float dx = point1.X - point2.X;
            float dy = point1.Y - point2.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        float MaxDistance(Vector2 v)
        {
            float res = list.Min(n => DistanceBetweenTwoPoints(n., v));
            return res;
        }

        /*IEnumerable<DataItem> MaxDistanceItems(Vector2 v)
        {

        }

        IEnumerable<DataItem>*/
       
    }

    public struct DataItem
    {
        public Vector2 coordinate { get; set; }
        public Vector2 value { get; set; }

        public DataItem(Vector2 coord, Vector2 val)
        {
            coordinate = coord;
            value = val;
        }

        public override string ToString()
        {
            return coordinate.ToString() + " " + value.ToString();
        }

        public string ToString(string format)
        {
            return coordinate.ToString(format) + " " + value.ToString(format);
        }
    }

    class prog
    {
        public static int Main()
        {
        /*Console.WriteLine("TASK 1\n\n");
        Grid2D item = new Grid2D(1, 1, 2, 2);
        V5DataOnGrid obj = new V5DataOnGrid("", DateTime.Now, item);
        obj.InitRandom(1, 5);
        Console.WriteLine(obj.ToLongString());
        V5DataCollection obj1 = (V5DataCollection)obj;
        Console.WriteLine(obj1.ToLongString());

        Console.WriteLine("TASK 2\n\n");
        V5MainCollection obj2 = new V5MainCollection();
        obj2.AddDefaults();
        Console.WriteLine(obj2.ToString());
        Console.WriteLine("TASK 3\n\n");
        Vector2[] array;
        foreach (V5Data ob in obj2)
        {
            array = ob.NearEqual(2);
            for (int i = 0; i < array.Length; i++)
            {
                Console.WriteLine(array[i].X + " " + array[i].Y + "\n");
            }
        }
        Console.WriteLine(obj2.ToString());*/
        Nikolskiy_lab2.V5DataCollection test = new Nikolskiy_lab2.V5DataCollection("", DateTime.Now, "file.txt");
        Console.WriteLine(test.ToLongString(format));
        return 0;
        }
    }
}
