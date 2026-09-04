using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SOLTIUS_Scheduler_Add_On.UI
{
    public partial class FormPassword : Form
    {
        public FormPassword()
                {
                    InitializeComponent();
                    UITheme.ApplyForm(this); // center screen — StartPosition harus di-set sebelum Show
                    // Menghubungkan event klik tombol save
                    this.btnsave.Click += new System.EventHandler(this.btnsave_Click);

            // Opsional: Sembunyikan karakter saat mengetik password
            txtpass.UseSystemPasswordChar = true;
            txtpass2.UseSystemPasswordChar = true;
        }

        private void FormPassword_Load(object sender, EventArgs e)
        {
            // Terapkan theme konsisten
            UITheme.ApplyForm(this);
            UITheme.ApplyPrimary(btnsave);
            UITheme.ApplyTextBox(txtpass);
            UITheme.ApplyTextBox(txtpass2);

            try
            {
                // 1. Tentukan path file XML
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Security", "AccessConfig.xml");

                // 2. Cek apakah file ada
                if (File.Exists(configPath))
                {
                    PasswordData storedData;
                    XmlSerializer serializer = new XmlSerializer(typeof(PasswordData));

                    using (StreamReader reader = new StreamReader(configPath))
                    {
                        storedData = (PasswordData)serializer.Deserialize(reader);
                    }

                    // 3. Decode password (AES format baru, fallback Base64 format lama)
                    if (storedData != null && !string.IsNullOrEmpty(storedData.EncryptedKey))
                    {
                        string decryptedPassword = AccessPasswordStore.Decrypt(storedData.EncryptedKey);

                        // 4. Masukkan ke TextBox
                        txtpass.Text = decryptedPassword;
                        txtpass2.Text = decryptedPassword;

                        // 5. MIGRASI OTOMATIS: kalau masih format lama (Base64), tulis ulang ke AES
                        if (AccessPasswordStore.IsLegacyFormat(storedData.EncryptedKey))
                        {
                            storedData.EncryptedKey = AccessPasswordStore.Encrypt(decryptedPassword);
                            SavePasswordToXml(storedData);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Opsional: Log error jika gagal membaca file
                MessageBox.Show("Gagal memuat password lama: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnsave_Click(object sender, EventArgs e)
        {
            try
            {
                // 1. Validasi Input Kosong
                if (string.IsNullOrWhiteSpace(txtpass.Text) || string.IsNullOrWhiteSpace(txtpass2.Text))
                {
                    MessageBox.Show("Password tidak boleh kosong!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // 2. Validasi Match Password
                if (txtpass.Text != txtpass2.Text)
                {
                    MessageBox.Show("Confirm Password tidak cocok. Silakan cek kembali.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 3. Mapping ke Object (Enkripsi AES, bukan Base64)
                string plainPassword = txtpass.Text;
                string encodedPassword = AccessPasswordStore.Encrypt(plainPassword);

                PasswordData data = new PasswordData
                {
                    SavedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    // Simpan sebagai AES (terenkripsi), bukan Base64 yang bisa dibaca siapa saja
                    EncryptedKey = encodedPassword
                };

                // 4. Proses Simpan ke XML
                SavePasswordToXml(data);

                MessageBox.Show("Password berhasil diperbarui dan disimpan ke XML!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Tutup form setelah berhasil
            }
            catch (Exception ex)
            {
                MessageBox.Show("Gagal menyimpan password: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SavePasswordToXml(PasswordData data)
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Security");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, "AccessConfig.xml");

            XmlSerializer serializer = new XmlSerializer(typeof(PasswordData));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, data);
            }
        }
    }

    // Model untuk XML
    public class PasswordData
    {
        public string SavedDate { get; set; }
        public string EncryptedKey { get; set; }
    }
}