using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Windows.Forms;

namespace AlphaIMS
{
    class TableXml
    {
        static XmlDocument _doc = new XmlDocument();
        static string _xmlFile = Application.StartupPath + "\\table.xml";

        public static void Init()
        {
            _doc.Load(_xmlFile);
        }

        public static void Deinit()
        {
            _doc = null;
        }

        public static void GetStations(ref List<string> stations)
        {
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string name = nodeList[i].FirstChild.InnerText.Trim();
                stations.Add(name);
            }
        }

        public static void GetModel(string station, ref List<string> models)
        {
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                if (parent.CompareTo(station) == 0)
                {
                    string name = nodeList[i].FirstChild.InnerText.Trim();
                    models.Add(name);
                }
            }
        }

        public static void GetCustomers(string station, string model, ref List<string> customers)
        {
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                if (grandparent.CompareTo(station) == 0 && parent.CompareTo(model) == 0)
                {
                    string name = nodeList[i].FirstChild.InnerText.Trim();
                    customers.Add(name);
                }
            }
        }

        public static void InsertProduct(string station, string model, string customer, int rows, int columns)
        {
            XmlNode rowsNode, columnsNode, customerNode, modelNode, stationNode, root;

            rowsNode = _doc.CreateElement("Rows");
            rowsNode.AppendChild(_doc.CreateTextNode(rows.ToString()));

            columnsNode = _doc.CreateElement("Columns");
            columnsNode.AppendChild(_doc.CreateTextNode(columns.ToString()));

            customerNode = _doc.CreateElement("Customer");
            customerNode.AppendChild(_doc.CreateTextNode(customer));

            modelNode = _doc.CreateElement("Model");
            modelNode.AppendChild(_doc.CreateTextNode(model));

            stationNode = _doc.CreateElement("Station");
            stationNode.AppendChild(_doc.CreateTextNode(station));

            customerNode.AppendChild(rowsNode);
            customerNode.AppendChild(columnsNode);
            modelNode.AppendChild(customerNode);
            stationNode.AppendChild(modelNode);

            root = _doc.DocumentElement.SelectSingleNode("/Tables/Products");
            if (root != null)
            {
                root.AppendChild(stationNode);
            }

            _doc.Save(_xmlFile);
        }

        public static int SelectProduct(string station, string model, string customer)
        {
            int result = 0;
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer");

            for (int i = 0; i < nodeList.Count; i++)
            {
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                string current = nodeList[i].FirstChild.InnerText.Trim();

                if (grandparent.CompareTo(station) == 0 && parent.CompareTo(model) == 0 && current.CompareTo(customer) == 0)
                {
                    result = 1;
                }
            }
            return result;
        }

        public static void GetProduct(string station, string model, string customer, ref int rows, ref int columns, ref List<TABLECOLUMN> tableColList)
        {
            double min, max;
            int value;
            TABLECOLUMN tableCol;
            XmlNode child;
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/Rows");

            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    string name = nodeList[i].FirstChild.InnerText.Trim();
                    Int32.TryParse(name, out rows);
                    break;
                }
            }

            nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/Columns");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    string name = nodeList[i].FirstChild.InnerText.Trim();
                    Int32.TryParse(name, out columns);
                    break;
                }
            }

            nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/column");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    child = nodeList[i].FirstChild;
                    tableCol = new TABLECOLUMN();

                    child = child.NextSibling;
                    while (child != null)
                    {
                        if (child.Name.CompareTo("name") == 0)
                            tableCol.Name = child.InnerText.Trim();
                        else if (child.Name.CompareTo("formula") == 0)
                            tableCol.Formula = child.InnerText.Trim();
                        else if (child.Name.CompareTo("min") == 0)
                        {
                            Double.TryParse(child.InnerText.Trim(), out min);
                            tableCol.Min = min;
                        }
                        else if (child.Name.CompareTo("max") == 0)
                        {
                            Double.TryParse(child.InnerText.Trim(), out max);
                            tableCol.Max = max;
                        }
                        else if (child.Name.CompareTo("readvalue") == 0)
                        {
                            Int32.TryParse(child.InnerText.Trim(), out value);
                            tableCol.Readvalue = value;
                        }

                        child = child.NextSibling;
                    }

                    tableColList.Add(tableCol);
                }
            }  
        }

        public static void SaveProduct(string station, string model, string customer, int rows, int columns)
        {
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/Rows");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();

                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    nodeList[i].FirstChild.InnerText = rows.ToString();
                    break;
                }
            }

            nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/Columns");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();

                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    nodeList[i].FirstChild.InnerText = columns.ToString();
                    break;
                }
            }

            _doc.Save(_xmlFile);
        }

        public static void DeletProduct(string station, string model, string customer)
        {
            XmlNode productNode, stationNode, modelNode, customerNode;
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/Rows");
            for (int i = 0; i < nodeList.Count; i++)
            {
                productNode = nodeList[i].ParentNode.ParentNode.ParentNode.ParentNode;
                stationNode = nodeList[i].ParentNode.ParentNode.ParentNode;
                modelNode = nodeList[i].ParentNode.ParentNode;
                customerNode = nodeList[i].ParentNode;

                string parent1 = stationNode.FirstChild.InnerText.Trim();
                string parent2 = modelNode.FirstChild.InnerText.Trim();
                string parent3 = customerNode.FirstChild.InnerText.Trim();
                if (parent1.CompareTo(station) == 0 && parent2.CompareTo(model) == 0 && parent3.CompareTo(customer) == 0)
                {
                    modelNode.RemoveChild(customerNode);

                    if (modelNode.ChildNodes.Count <= 1)
                        stationNode.RemoveChild(modelNode);

                    if (stationNode.ChildNodes.Count <= 1)
                        productNode.RemoveChild(stationNode);

                    break;
                }
            }

            _doc.Save(_xmlFile);
        }
        
        public static void InsertColumnInfo(string station, string model, string customer, int column, TABLECOLUMN tableCol)
        {
            XmlNode tabColNode, nameNode, formulaNode, minNode, maxNode, readvalueNode;
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer");

            for (int i = 0; i < nodeList.Count; i++)
            {
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();
                string current = nodeList[i].FirstChild.InnerText.Trim();

                if (grandparent.CompareTo(station) == 0 && parent.CompareTo(model) == 0 && current.CompareTo(customer) == 0)
                {
                    tabColNode = _doc.CreateElement("column");
                    tabColNode.AppendChild(_doc.CreateTextNode(column.ToString()));

                    nameNode = _doc.CreateElement("name");
                    nameNode.AppendChild(_doc.CreateTextNode(tableCol.Name));

                    formulaNode = _doc.CreateElement("formula");
                    formulaNode.AppendChild(_doc.CreateTextNode(tableCol.Formula));

                    minNode = _doc.CreateElement("min");
                    minNode.AppendChild(_doc.CreateTextNode(tableCol.Min.ToString()));

                    maxNode = _doc.CreateElement("max");
                    maxNode.AppendChild(_doc.CreateTextNode(tableCol.Max.ToString()));

                    readvalueNode = _doc.CreateElement("readvalue");
                    readvalueNode.AppendChild(_doc.CreateTextNode(tableCol.Readvalue.ToString()));

                    tabColNode.AppendChild(nameNode);
                    tabColNode.AppendChild(formulaNode);
                    tabColNode.AppendChild(minNode);
                    tabColNode.AppendChild(maxNode);
                    tabColNode.AppendChild(readvalueNode);

                    nodeList[i].AppendChild(tabColNode);
                }
            }

            _doc.Save(_xmlFile);
        }

        public static void SaveColumnInfo(string station, string model, string customer, int column, TABLECOLUMN tableCol)
        {
            int value;
            XmlNode child;
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/column");

            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();

                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    child = nodeList[i].FirstChild;
                    Int32.TryParse(child.InnerText.Trim(), out value);

                    if (value == column)
                    {
                        child = child.NextSibling;
                        while (child != null)
                        {
                            if (child.Name.CompareTo("name") == 0)
                                child.InnerText = tableCol.Name;
                            else if (child.Name.CompareTo("formula") == 0)
                                child.InnerText = tableCol.Formula;
                            else if (child.Name.CompareTo("min") == 0)
                                child.InnerText = tableCol.Min.ToString("#0.00");
                            else if (child.Name.CompareTo("max") == 0)
                                child.InnerText = tableCol.Max.ToString("#0.00");
                            else if (child.Name.CompareTo("readvalue") == 0)
                                child.InnerText = tableCol.Readvalue.ToString();

                            child = child.NextSibling;
                        }

                        break;
                    }
                }
            }

            _doc.Save(_xmlFile);
        }

        public static void DeleteColumnInfo(string station, string model, string customer, int column)
        {
            bool readjust = false;
            int value;
            XmlNode child;
            XmlNode parentNode;
            XmlNodeList nodeList = _doc.DocumentElement.SelectNodes("/Tables/Products/Station/Model/Customer/column");

            for (int i = 0; i < nodeList.Count; i++)
            {
                string great_grandparent = nodeList[i].ParentNode.ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string grandparent = nodeList[i].ParentNode.ParentNode.FirstChild.InnerText.Trim();
                string parent = nodeList[i].ParentNode.FirstChild.InnerText.Trim();

                if (great_grandparent.CompareTo(station) == 0 && grandparent.CompareTo(model) == 0 && parent.CompareTo(customer) == 0)
                {
                    child = nodeList[i].FirstChild;
                    Int32.TryParse(child.InnerText.Trim(), out value);

                    if (readjust)
                    {
                        child.InnerText = (value - 1).ToString();
                    }

                    if (value == column)
                    {
                        readjust = true;
                        parentNode = nodeList[i].ParentNode;
                        parentNode.RemoveChild(nodeList[i]);
                    }
                }
            }

            _doc.Save(_xmlFile);
        }

        public static void SaveData(string station, string model, string customer, string data, string time)
        {
        }
    }
}
