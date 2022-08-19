namespace ConvertSqliteToMSSQL
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            string output = "SORRY UNABLE TO READ FILE";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                string file = openFileDialog1.FileName;
                try
                {
                    string text = File.ReadAllText(file);

                    text = text.Replace("BEGIN TRANSACTION;", "USE MASTER;\nGO\nDROP DATABASE IF EXISTS jimstest;\nGO\nCREATE DATABASE jimstest;\nGO\nUSE jimstest;\nGO");
                    text = text.Replace("IF NOT EXISTS ", "");

                    //Replace AUTOINCREMENT with IDENTITY
                    foreach (string line in GetBetweenAsArray(text, "CREATE TABLE", ");", true))
                    {
                        string AUTOINCREMENT_key = GetBetweenAsString(line, "PRIMARY KEY(\"", "\" AUTOINCREMENT)", false);
                        string AUTOINCREMENT_type = GetBetweenAsString(line, "	\"" + AUTOINCREMENT_key + '"', "\n", false).Trim();

                        if (AUTOINCREMENT_type != "")
                        {
                            string newline = line.Replace(AUTOINCREMENT_type, AUTOINCREMENT_type.Replace(",", " IDENTITY(1,1),"));
                            text = text.Replace(line, newline + "\n");
                        }

                    }

                    //edit INSERT INTO
                    string last = "";
                    foreach (string line in GetBetweenAsArray(text, "INSERT INTO ", " VALUES", false)) {

                        //DO NOT DO THE SAME TASK MORE THEN ONCE
                        if (last == line)
                        {
                            continue;
                        }

                        string lookup = GetBetweenAsString(text, "CREATE TABLE " + line, ");", false);
	                    if (lookup != "") {
                            string[] lookup2 = lookup.Split("\n");

		                    string colomns = "";
                            for (int x = 1; x < (lookup2.GetLength(0) - 2); x++) {
                                colomns = colomns + GetBetweenAsString(lookup2[x], "\"", "\"", false) + ",";
		                    }

                            string IDENTITY_INSERT = "";
                            if (last == "") {
                                IDENTITY_INSERT = "SET IDENTITY_INSERT " + line + " ON;\n";
                            } else {
                                IDENTITY_INSERT = "SET IDENTITY_INSERT " + last + " OFF;\nSET IDENTITY_INSERT " + line + " ON;\n";
                            }
		                    last = line;

                            colomns = colomns.Substring(0, colomns.Length - 1); //hope this is the same as in php substr

                            text = text.Replace("INSERT INTO " + line + " VALUES", IDENTITY_INSERT + "INSERT INTO " + line.Replace("\n", "") + " (" + colomns + ") VALUES");

	                    }
                    }

                    text = text.Replace(" AUTOINCREMENT", "");
                    text = text.Replace("COMMIT;", "");
                    //text = text.Replace("", "");

                    output = ""; //clear output and add data later

                    List<string> exists = new List<string>();
                    foreach (string line in text.Split("\n")) {
                        string lookup = GetBetweenAsString(line, "SET IDENTITY_INSERT \"", "\" ON;");
                        if (lookup != "" && !exists.Contains(lookup))
                        {
                            exists.Add(lookup);
                            output = output + line + "\n";

                        }
                        else if (lookup != "" && exists.Contains(lookup))
                        {
                            //remove line
                        }
                        else
                        {
                            output = output + line + "\n";
                        }
                    }

                    /* Echo found
                    output = ""; //clear output and add data later
                    foreach (string line in exists) {
                        output = output + line + "\n";
                    }
                    */

                }
                catch (Exception)
                {
                    //throw; //add to debug
                }
            }

            richTextBox1.Text = output; // <-- Shows file size in debugging mode.
        }

        public static string GetBetweenAsStringInWorkProgress(string data, string start, string end, bool include_start_end = false)
        {
            string FinalString;
            int Pos1 = data.IndexOf(start) + start.Length;
            int Pos2 = data.IndexOf(end);
            if (Pos1 > Pos2 || Pos2 <= 0) {
                return "";
            }
            FinalString = data.Substring(Pos1, Pos2 - Pos1);
            if (include_start_end == true)
            {
                return start + FinalString + end;
            }
            return FinalString;
        }

        private static string GetBetweenAsString(string data, string start, string end, bool include_start_end = false)
        {
            if (data.Split(start).Length > 1)
            {
                string pointer = data.Split(start)[1];
                pointer = pointer.Split(end)[0];
                if (include_start_end == true)
                {
                    return start + pointer + end;
                }
                return pointer;
            }
            else {
                return "";
            }
        }

        private static string[] GetBetweenAsArray(string data, string start, string end, bool include_start_end = false)
        {
            List<string> output = new List<string>();
            string[] pointer = data.Split(start);
            for (int x=1;x<pointer.GetLength(0);x++)
            {
                if (include_start_end == true)
                {
                    output.Add(start + pointer[x].Split(end)[0] + end);
                }
                else
                {
                    output.Add(pointer[x].Split(end)[0]);
                }
            }
            
            return output.ToArray();
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {

        }
    }
}