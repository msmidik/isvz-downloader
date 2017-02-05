using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VestnikVZClient.zakazky;
using System.Configuration;
using System.IO;

namespace VestnikVZClient
{
    public class ExportClient
    {
        private ExportFormsClient client = new ExportFormsClient();
        private Guid guid = new Guid(ConfigurationManager.AppSettings["FullId"]);   // dev.test id
        private const int DEFAULT_STEP = 40;
        private const string FORMAT_VERSION = "7.2";
        private const string FILE_PATH = @"seznam.txt";
        private const string EXPORT_DIR_PATH = "export";
        private const string FILE_NAME = "text";
        private const string ERROR_FILE_PATH = @"seznam-chyby.txt";

        /// <summary>
        /// get version
        /// </summary>
        public void GetVersion()
        {
            var request = new GetVersionRequest();
            var response = new GetVersionResponse();

            request.UserId = guid;
            response = client.GetVersion(request);
            client.Close();

            var message = response.Message;
            var version = response.Version.Version;

            Console.WriteLine(message);
            Console.WriteLine("verze " + version);
            Console.ReadLine();
        }


        /// <summary>
        /// get list of forms
        /// </summary>
        /// <param name="fromDate">from date</param>
        /// <param name="toDate">to date</param>
        public void GetList(DateTime fromDate, DateTime toDate)
        {
            var request = new ListFormsRequest();
            var response = new ListFormsResponse();
            QueryParameter[] queryParameters = { new QueryParameter(), new QueryParameter() };
            queryParameters[0].Name = "DateTimePublicationFrom";
            queryParameters[1].Name = "DateTimePublicationTo";
            request.UserId = guid;
            
            int step = Math.Min(DEFAULT_STEP, (toDate - fromDate).Days);

            for (var date = fromDate; DateTime.Compare(date.AddDays(step), toDate) <= 0; date = date.AddDays(step))
            {
                string startDate = date.ToString("M.d.yyyy");
                string endDate = date.AddDays(step).ToString("M.d.yyyy");
                queryParameters[0].Value = startDate;
                queryParameters[1].Value = endDate;
                request.QueryParameters = queryParameters;

                try
                {
                    response = client.ListForms(request);

                    string message = response.Message;
                    Console.WriteLine("seznam " + date.ToString("d.M.yyyy") + " " + date.AddDays(step).ToString("d.M.yyyy") + " : " + message);
                    var forms = response.Forms; 

                    using (StreamWriter sw = new StreamWriter(FILE_PATH, true))
                    {
                        foreach (var f in forms)
                        {
                            sw.WriteLine(f.Id);
                        }
                    }
                    Console.WriteLine("počet formulářů: " + forms.Length);
                }
                catch (System.Exception)
                {
                    step = Math.Max(1,step / 2);
                    Console.WriteLine("chyba, měním krok na " + Convert.ToString(step));
                    date = date.AddDays(-step);
                }
                step = Math.Min(DEFAULT_STEP, (toDate - fromDate).Days);

            }
            client.Close();
            Console.ReadLine();
            
        }


        /// <summary>
        /// get form
        /// </summary>
        public void GetForm()
        {

            var request = new GetFormDocumentRequest();
            var response = new GetFormDocumentResponse();

            request.UserId = guid;
            request.FormatVersion = FORMAT_VERSION;


            var formsIdList = new List<int>();
            using (StreamReader sr = new StreamReader(FILE_PATH))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    formsIdList.Add(int.Parse(line));
                }
            }

            formsIdList = formsIdList.Distinct().ToList();
            int formsNumber = formsIdList.Count;


            Directory.CreateDirectory(EXPORT_DIR_PATH); 

            for (var a = 0; a < formsNumber; a++)
            {
                int formId = formsIdList[a];
                request.FormId = formId;

                try
                {
                    response = client.GetFormDocument(request);
                    string message = response.Message;

                    string form = Encoding.UTF8.GetString(response.FormDocument);

                    File.WriteAllText(EXPORT_DIR_PATH + @"\" + FILE_NAME + formId + ".xml", form);

                    Console.WriteLine(Convert.ToString(a + 1) + "/" + formsNumber + " form " + formId + " : " + message);
                }
                catch (System.Exception)
                {
                    Console.WriteLine("chyba: " + Convert.ToString(formId));
                    using (StreamWriter sw = new StreamWriter(ERROR_FILE_PATH, true))
                    {
                        sw.WriteLine(formId);
                    }
                }

            }
            client.Close();
            Console.ReadLine();
            
        }

    }
}
