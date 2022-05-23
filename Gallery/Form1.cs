using Amazon.Rekognition.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Gallery
{
    public partial class Form1 : Form
    {
        int index = 0;
        Root root = new Root();
        string url = "https://pixabay.com/api/?key=18092743-c6624b09a26fd86824b499689&q=" + "flowers" + "&image_type=photo";
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            this.pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            LoadPicture();
        }
        private void backBtn_Click(object sender, EventArgs e)
        {
            if (index - 1 >= 0 && root.hits.Count > 0)
            {
                this.pictureBox1.Load(root.hits[--index].previewURL);
            }
        }

        private void nextBtn_Click(object sender, EventArgs e)
        {
            if (index + 1 < root.hits.Count)
            {
                this.pictureBox1.Load(root.hits[++index].previewURL);
            }
        }
        private async Task<string> SendRequest()
        {
            HttpClient httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync(url).Result;

            return await response.Content.ReadAsStringAsync();
        }
        private void LoadPicture()
        {
            root = JsonConvert.DeserializeObject<Root>(SendRequest().Result);
            if (root.hits.Count > 0)
            {
                index = 0;
                this.pictureBox1.Load(root.hits[index].previewURL);
            }
            else
            {
                MessageBox.Show("No images found!");
            }
        }

        private void searchBtn_Click(object sender, EventArgs e)
        {
            List<string> words = this.searchTB.Text.Split(new char[] { ' ', ':', ',', '-', '+', '-', ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            string keywords = string.Empty;
            foreach (string word in words)
            {
                keywords += word + "+";
            }
            if (keywords.Length > 0)
            {
                keywords = keywords.Remove(keywords.Length - 1);
            }
            url = "https://pixabay.com/api/?key=18092743-c6624b09a26fd86824b499689&q=" + keywords + "&image_type=photo";
            LoadPicture();
        }

        public async Task Main()
        {
            string photo = root.hits[index].previewURL;

            var awsCredentials = new Amazon.Runtime.BasicAWSCredentials("AKIA3BZMVBVKD2IHGKEZ", "rlTB6Ne7BJxDgX1nCOrMU98uDVdP8ux/VZkrgPqe");
            var rekognitionClient = new Amazon.Rekognition.AmazonRekognitionClient(awsCreden‌tials, Amazon.RegionEndpoint.USEast1);

            var image = new Amazon.Rekognition.Model.Image();
            try
            {
                ImageConverter converter = new ImageConverter();
                byte[] data = (byte[])converter.ConvertTo(pictureBox1.Image, typeof(byte[]));
                image.Bytes = new MemoryStream(data);

            }
            catch (Exception)
            {
                MessageBox.Show("Failed to load file " + photo);
                return;
            }
            var detectlabelsRequest = new DetectLabelsRequest()
            {
                Image = image,
                MaxLabels = 10,
                MinConfidence = 75F,
            };

            try
            {
                DetectLabelsResponse detectLabelsResponse = await rekognitionClient.DetectLabelsAsync(detectlabelsRequest);
                
                if (detectLabelsResponse.Labels.Any(label => label.Name == comboBox1.SelectedItem.ToString()))
                {
                    detectLabelsResponse.Labels.Where(label => label.Name == comboBox1.SelectedItem.ToString()).First().Instances.ForEach(instance =>
                        ShowBoundingBoxPositions(instance.BoundingBox));
                    MessageBox.Show($"Found: {comboBox1.SelectedItem}");
                }
                else
                {
                    MessageBox.Show("No items found!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        public void ShowBoundingBoxPositions(BoundingBox box)
        {
            float top = (box.Top * pictureBox1.Height);
            float left = (box.Left * pictureBox1.Width);
            float right = pictureBox1.Width * box.Width;
            float bottom = pictureBox1.Height * box.Height;
            Pen pen = new Pen(Color.Aqua, 2);
            using (Graphics g = pictureBox1.CreateGraphics())
            {
                g.DrawRectangle(pen, left, top, right, bottom);
            }
        }

        private void findItemBtn_Click(object sender, EventArgs e)
        {
            Main();
        }
    }
}
