namespace WinFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(OutputBox.Text))
                Directory.Delete(OutputBox.Text, true);
            var files = Directory.GetFiles(TemplateBox.Text, "*.png", searchOption: SearchOption.AllDirectories);

            for (int i = 0; i < files.Length; i++)
            {
                var relPath = Path.GetRelativePath(TemplateBox.Text, files[i]);

                TextBox nameBox;
                TextBox countBox;
                switch (i)
                {
                    case 0:
                        nameBox = Name0;
                        countBox = Count0;
                        break;
                    case 1:
                        nameBox = Name1;
                        countBox = Count1;
                        break;
                    case 2:
                        nameBox = Name2;
                        countBox = Count2;
                        break;
                    case 3:
                        nameBox = Name3;
                        countBox = Count3;
                        break;
                    default:
                        nameBox = Name4;
                        countBox = Count4;
                        break;
                }
                nameBox.Text = relPath;
                if (countBox.Text == "")
                    countBox.Text = "1";

                var count = int.Parse(countBox.Text);
                var pathPrefix = Path.Combine(OutputBox.Text, relPath[..^4]);
                Directory.CreateDirectory(Path.GetDirectoryName(pathPrefix));
                for (int j = 0; j < count; j++)
                {
                    File.Copy(files[i], $"{pathPrefix}{j}.png");
                    File.SetLastWriteTime($"{pathPrefix}{j}.png", DateTime.Now);
                }
            }
        }

        private void Name3_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
