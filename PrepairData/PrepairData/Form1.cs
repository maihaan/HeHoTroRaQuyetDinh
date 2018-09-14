using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace PrepairData
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btBrowse_Click(object sender, EventArgs e)
        {
            OpenFileDialog od = new OpenFileDialog();
            od.Multiselect = false;
            if(od.ShowDialog() == DialogResult.OK)
            {
                tbPath.Text = od.FileName;
            }
               
        }

        private void btRead_Click(object sender, EventArgs e)
        {
            String[] lines = System.IO.File.ReadAllLines(tbPath.Text);
            List<String> events = new List<string>();
            foreach(String line in lines)
            {
                // Lay tat ca cac su kien
                if(line.StartsWith("[2]") || line.StartsWith("[3]"))
                {
                    String data = line.Substring(4);
                    foreach(String ev in data.Split(','))
                    {
                        if (!events.Contains(ev.Trim()))
                            events.Add(ev.Trim());
                    }
                }
            }

            // Cap nhat cac su kien vao CSDL
            DataTable tb = ReadData("Select Max(Ma) as MaLN from tbSuKien");
            int maLN = 0;
            if (tb != null && tb.Rows.Count > 0)
                int.TryParse(tb.Rows[0]["MaLN"].ToString(), out maLN);

            foreach(String ev in events)
            {
                if(!String.IsNullOrEmpty(ev))
                {
                    maLN++;
                    String query = "Insert into tbSuKien(Ten,Ma) Values(N'" + ev + "'," + maLN.ToString() + ")";
                    WriteData(query);
                }                
            }

            // Cap nhat cac luat vao CSDL
            for(int i = 0; i < lines.Length; i++)
            {
                if(lines[i].StartsWith("[1]"))
                {
                    String tenLuat = lines[i].Substring(4);
                    String veTrai = lines[i + 1].Substring(4);
                    String vePhai = lines[i + 2].Substring(4);

                    // Tach ve phai neu ve phai co nhieu hon 1 su kien
                    foreach(String eRight in vePhai.Split(','))
                    {
                        if (!String.IsNullOrEmpty(eRight))
                        {
                            // Lay ma lon nhat trong csdl luat
                            DataTable tb1 = ReadData("Select Max(Ma) as MaLN from tbLuat");
                            int maLuatLN = 0;
                            if (tb1 != null && tb1.Rows.Count > 0)
                                int.TryParse(tb1.Rows[0]["MaLN"].ToString(), out maLuatLN);

                            // Lay ma cua su kien o ve phai
                            DataTable tbERight = ReadData("Select Ma From tbSuKien Where Ten=N'" + eRight.Trim() + "'");
                            int maERight = int.Parse(tbERight.Rows[0]["Ma"].ToString());

                            // tach ra thanh nhieu dong nhung van la 1 luat
                            foreach (String eLeft in veTrai.Split(','))
                            {
                                // Lay ma cua su kien o ve trai
                                DataTable tbELeft = ReadData("Select Ma From tbSuKien Where Ten=N'" + eLeft.Trim() + "'");
                                int maELeft = int.Parse(tbELeft.Rows[0]["Ma"].ToString());

                                String queryLuat = "Insert into tbLuat(Ten, Ma, VeTrai, VePhai) Values(N'"
                                    + tenLuat + "', " + (maLuatLN + 1).ToString() + "," + maELeft.ToString() + "," + maERight.ToString() + ")";
                                WriteData(queryLuat);
                            }
                        }
                    }
                    

                }
            }
            MessageBox.Show("OK");
        }

        private int WriteData(String query)
        {
            String connectionString = "Data Source=.\\MSSQL; Initial Catalog=DSS; user id=admin; Password=@n123456;";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, con);
            try
            {
                con.Open();
                int dem = cmd.ExecuteNonQuery();
                con.Close();
                return dem;
            }
            catch
            {
                return -1;
            }
        }

        private DataTable ReadData(String query)
        {
            String connectionString = "Data Source=.\\MSSQL; Initial Catalog=DSS; user id=admin; Password=@n123456;";
            SqlConnection con = new SqlConnection(connectionString);
            SqlCommand cmd = new SqlCommand(query, con);
            DataTable tb = new DataTable();
            try
            {
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                tb.Load(dr, LoadOption.OverwriteChanges);
                con.Close();
                return tb;
            }
            catch
            {
                return null;
            }
        }
    }
}
