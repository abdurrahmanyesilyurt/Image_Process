using System;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Data;
using ExcelDataReader;
using CsvHelper;
using System.Globalization;
using System.Collections.Generic;

class Program
{
    static async Task Main(string[] args)
    {
        string? Deskop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        // Excel dosyasının yolu

        string excelFilePath = @$"{Deskop}\crop_1.xlsx";

        // Geçerli resimlerin boyutlarının kaydedileceği CSV dosyasının yolu
        string outputCsvPath = @$"{Deskop}\PhotoDimensions.csv";

        // Hatalı URL'lerin ID'lerinin kaydedileceği CSV dosyasının yolu
        string invalidCsvPath = @$"{Deskop}\InvalidIds.csv";

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        List<UrlRecord> urlRecords = new List<UrlRecord>();

        // Excel dosyasını okuma
        using (var stream = File.Open(excelFilePath, FileMode.Open, FileAccess.Read))
        {
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                var result = reader.AsDataSet();

                // İlk sayfayı al
                var table = result.Tables[0];

                foreach (DataRow row in table.Rows)
                {
                    string id = row[0].ToString();
                    string path = row[1].ToString();

                    urlRecords.Add(new UrlRecord { Id = id, Path = path });
                }
            }
        }

        using (var validWriter = new StreamWriter(outputCsvPath))
        using (var invalidWriter = new StreamWriter(invalidCsvPath))
        using (var csvValidWriter = new CsvWriter(validWriter, CultureInfo.InvariantCulture))
        using (var csvInvalidWriter = new CsvWriter(invalidWriter, CultureInfo.InvariantCulture))
        {
            // Geçerli resimlerin boyut bilgilerini içeren CSV dosyasına başlıkları yaz
            csvValidWriter.WriteField("Id");
            csvValidWriter.WriteField("Width");
            csvValidWriter.WriteField("Height");
            csvValidWriter.NextRecord();

            // Hatalı URL'lerin ID'lerinin kaydedileceği CSV dosyasına başlık ekle
            csvInvalidWriter.WriteField("Id");
            csvInvalidWriter.NextRecord();

            foreach (var record in urlRecords)
            {
                Console.WriteLine($"İşlenen ID: {record.Id}");

                // URL'nin mutlak (absolute) bir URI olup olmadığını kontrol edin
                if (Uri.IsWellFormedUriString(record.Path, UriKind.Absolute))
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            // Resmi URL'den indir
                            var imageStream = await client.GetStreamAsync(record.Path);
                            using (var img = Image.FromStream(imageStream))
                            {
                                int width = img.Width;
                                int height = img.Height;

                                // Geçerli resimlerin sadece ID ve boyut bilgilerini yaz
                                csvValidWriter.WriteField(record.Id);
                                csvValidWriter.WriteField(width);
                                csvValidWriter.WriteField(height);
                                csvValidWriter.NextRecord();
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Resim indirilirken veya işlenirken hata oluştu: {record.Id}");
                        Console.WriteLine("2 e girdi");

                        // Hatalı resimlerin ID'lerini yaz
                        csvInvalidWriter.WriteField(record.Id);
                        csvInvalidWriter.NextRecord();
                    }
                }
                else
                {
                    Console.WriteLine($"Geçersiz URL formatı: {record.Path}");
                    Console.WriteLine("3 e girdi");
                    // Geçersiz formatta olan URL'lerin ID'lerini yaz

                    csvInvalidWriter.WriteField(record.Id);
                    csvInvalidWriter.NextRecord();
                }
            }
        }

        Console.WriteLine("Fotoğrafların boyut bilgileri ve hatalı resimlerin ID'leri farklı CSV dosyalarına yazıldı.");
    }
}

public class UrlRecord
{
    public string Id { get; set; }
    public string Path { get; set; }
}

