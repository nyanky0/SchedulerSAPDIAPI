using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace SOLTIUS_Scheduler_Add_On.UI
{
    public partial class FormLogin : Form
    {
        public FormLogin()
        {
            InitializeComponent();
            // Menghubungkan event klik
            this.btnLogin.Click += new System.EventHandler(this.btnLogin_Click);
            this.button1.Click += new System.EventHandler(this.btnReset_Click);
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Security", "AccessConfig.xml");

                if (!File.Exists(configPath))
                {
                    MessageBox.Show("Konfigurasi keamanan tidak ditemukan!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // 1. Baca data dari XML
                PasswordData storedData;
                XmlSerializer serializer = new XmlSerializer(typeof(PasswordData));
                using (StreamReader reader = new StreamReader(configPath))
                {
                    storedData = (PasswordData)serializer.Deserialize(reader);
                }

                // 2. Decode password (AES format baru, fallback Base64 format lama)
                string decryptedStoredPassword = AccessPasswordStore.Decrypt(storedData.EncryptedKey);

                // 3. Validasi
                if (txtPassword.Text == decryptedStoredPassword)
                {
                    // LOGIN BERHASIL
                    this.Hide(); // Sembunyikan FormLogin agar tidak menumpuk

                    // 4. Buka FormSync
                    FormMain frmSync = new FormMain();

                    // Mengatur agar ketika FormSync ditutup, aplikasi benar-benar berhenti
                    frmSync.FormClosed += (s, args) => Application.Exit();

                    frmSync.Show();
                }
                else
                {
                    MessageBox.Show("Password salah!", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    txtPassword.Clear();
                    txtPassword.Focus();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            // Opsi untuk reset password jika diperlukan
            var result = MessageBox.Show("Apakah Anda ingin mereset password? Ini akan membuka form setup password.",
                                        "Reset Password", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                FormPassword frmPass = new FormPassword();
                frmPass.ShowDialog();
            }
        }
    }

    // Pastikan class model ini ada (bisa di file terpisah atau di bawah class form)
    // Harus sama strukturnya dengan yang ada di FormPassword
    /*
    public class PasswordData
    {
        public string SavedDate { get; set; }
        public string EncryptedKey { get; set; }
    }
    */
}