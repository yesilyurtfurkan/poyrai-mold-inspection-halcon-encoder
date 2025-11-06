

// +-------------------------------- NOTLARIM-----------------------------------+|
// |                                                                             |
// |    		            FURKAN YEŞİLYURT				                     |
// |                          GÖKHAN ŞAHİN                                       |
// |   1- Uygulamaya Lisans Ekle              					                 |
// |   2- Uygulamaya Karekod Ekle              					                 |
// |   3- Uygulama Ekran Kayıt ve Ekran Görüntüsü Al Ekle   			         | 
// |   4- Uygulamada Versiyonlarını Kaydet                                       | 
// |   5-ADdtolog int=5 ise log veritabanına kayıt eder                          |
// |   6-LABEL86--ANA EKRAN BİLDİRİM   	                                         |
// +------+------+------+------+------+------+------+------+------+------+-------+

using HalconDotNet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Threading;
using System.Windows.Forms;
using System.Text;
using System.Linq;
using S7.Net;

namespace ProMeasure
{
    public partial class Form1 : Form
    {

        #region variables

        string ImageName = "-";
        int f = 0;                 //tETİTK -ENCODER HESAPLAMALARI İÇİN
        float R = 0;               //tETİTK -ENCODER HESAPLAMALARI İÇİN
        float S1 = 0;              //tETİTK -ENCODER HESAPLAMALARI İÇİN
        float S2 = 0;              //tETİTK -ENCODER HESAPLAMALARI İÇİN
        int se1 = 0;               //tETİTK -ENCODER HESAPLAMALARI İÇİN
        int se2 = 0;                //tETİTK -ENCODER HESAPLAMALARI İÇİN
        bool login = false;
        bool T2 = false;           //tETİTK -ENCODER HESAPLAMALARI İÇİN
        bool T3 = false;           //tETİTK -ENCODER HESAPLAMALARI İÇİN
        bool T4 = false;           //tETİTK -ENCODER HESAPLAMALARI İÇİN
        int logCounter = 0;
        int logCounter2 = 0;
        int dosyaSayisi = 0;
        private Plc plc = null;
        bool PlcBaglanti;
        bool timerActive = false;
        FileInfo[] Files;
        string V_control = "";
        String Ksecilen = "";
        HDevEngine hDevEngine = new HDevEngine();
        HObject ho_RRecion;
        HDevProcedureCall hOpenGrabber1;
        HDevProcedureCall hOpenGrabber2;
        HDevProcedureCall hGrabImagea1;
        HDevProcedureCall hGrabImagea2;
        HDevProcedureCall hGrabImageu1;
        HDevProcedureCall hGrabImage2;
        HDevProcedureCall hDoControls;
        HDevProcedureCall hReadModels;
        public static string[] filearray = new string[120];
        bool cameraOpened = false;
        String Kamera = "0";
        const int RetryCount = 5;

        HTuple hv_AcqHandle1;
        HTuple hv_AcqHandle2;

        HTuple hv_Model1;
        HTuple hv_Model2;
        HTuple hv_Model3;
        HTuple hv_Model4;

        const int controlCount = 25;
        Color myRgbColor = new Color();
        Color myGriColor = new Color();
        HObject ho_Imagea1 = new HObject();
        HObject ho_Imagea2 = new HObject();
        HObject ho_Imageu1 = new HObject();

        HObject ho_Tile1 = new HObject();
        HObject ho_Tile2 = new HObject();

        HWindow[] HWindows1 = new HWindow[controlCount];
        HWindow[] HWindows2 = new HWindow[controlCount];
        string ustsol, ustsag, altsol, altsag;
        HRegion[] PRegions = new HRegion[4 * controlCount];

        string Myyol = Application.StartupPath + "\\Settings\\Recete\\";
        bool SystemReady = false;

        PerformanceCounter cpuCounter;

        PerformanceCounter ramCounter;

        #endregion

        #region FORM_SENDER

        public Form1()
        {

            InitializeComponent();

            AddToLog("Program", this.Text + " Started. ", 0, Color.Gray);

            // LoadConfigs();

            for (int i = 0; i < (4 * controlCount); i++)
            {

                PRegions[i] = null;
            }

            HWindows1[0] = hWin1.HalconWindow;
            HWindows1[1] = hWin2.HalconWindow;

            // LoadRegions();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            tmrAO.Enabled = true;
        }

        private void tmrAO_Tick(object sender, EventArgs e)
        {
            tmrAO.Enabled = false;

            bool h = InitHALCON();

            if (h)
            {
                ReadModels();
                bool c = InitCameras();

                bool d = InitCameras2();
                if (c)
                {

                    GrabNCheck(true);
                    SystemReady = true;
                }
                cameraOpened = c;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myRgbColor = Color.FromArgb(0, 120, 215);
            ustsol=ustsag=altsol=altsag="";
            myGriColor = Color.FromArgb(65, 70, 75);
            Controls_Tooltip("PLC BAĞLANTISI AKTİF DEĞİLDİR", "PLC AKTİF DEĞİLDİR.", button1);
            PlcBaglan();
            LoadIp();
            SetFontAndColors();
            dateTimePicker4.Value = DateTime.Now;
            SistemPerfomance();
            KayitliReceteResetle();
            TabClear(); //başlangıçta çalıştır 
            LoadConfigs();
            LoginPasif(true);
            this.TbAna.Parent = this.tabControl1; //show
            this.tbhomee.Parent = this.tabControl2; //show
            cb_Recete.Text = "Seçiniz...";
            string path3 = Application.StartupPath + "\\Settings\\Recete";
            getDirectories(path3, dataGridView1);
            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

        }

        private void TabClear()
        {
         
            this.tabPage1.Parent = null; // hide
            this.tblog.Parent = null; // hide
            this.tbreport.Parent = null; // hide
            this.tbpasword.Parent = null; // hide

            this.TbAna.Parent = null; // hide
            this.tbhomee.Parent = null;
            this.tbsettings.Parent = null;
            this.tbrecete.Parent = null;
            this.tb_edit.Parent = null;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            timerActive = false;
            Thread.Sleep(200);
            if (PlcBaglanti)
            {
                plc.Close();
            }
            // Application.DoEvents();
        }

        private void hwPos0_HMouseDown(object sender, HMouseEventArgs e)
        {
            HWindowControl hw = ((HWindowControl)sender);
            string sName = hw.Name;
            sName = sName.Remove(0, 2);
            string[] arr = sName.Split('_');
            int camNo = Convert.ToInt32(arr[0]);
            int posNo = Convert.ToInt32(arr[1]);
            //    LoadImage(camNo, posNo);
        }

        private void Bt_PrintRegion_Click(object sender, EventArgs e)
        {
            cb_Recete.Text = "Seçiniz...";
            string rDir = Myyol + Tx_RName.Text + "\\";
            ;
            try

            {
                dosyaSayisi = Directory.GetFiles(rDir, "*.hobj", SearchOption.AllDirectories).Length;
            }
            catch
            {
                dosyaSayisi = 0;
            }
            int idx = dosyaSayisi + 1;
            if (Cbk1.Checked)
            {
                PrintRegion(Tx_RName, idx);
            }
            else
            {
                PrintRegion2(Tx_RName, idx);
            }


        }

        private void button76_Click(object sender, EventArgs e)
        {
            if (Tx_RName.Text == "-")
            {
                MessageBox.Show("Reçete Adı Giriniz !", "Reçete Adı Giriniz ! ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (cb_Recete.Text == "Seçiniz...")
            {
                MessageBox.Show("Görev Ataması Yapınız !", "Görev Ataması Yapınız ! ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {

                DialogResult secenek = MessageBox.Show("Seçilen ROI Bölgesi'ne " + cb_Recete.Text.ToString() + " alan kontrolü kayıt edilsin mi", "BİLGİ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (secenek == DialogResult.Yes)
                {
                    string rDir = Myyol + Tx_RName.Text + "\\";

                    if (!Directory.Exists(rDir))
                        Directory.CreateDirectory(rDir);

                    int idx = dosyaSayisi + 1;
                    string MyValue = cb_Recete.Text.ToString();
                    SaveRoiLabels(rDir, MyValue, idx);
                }
                else if (secenek == DialogResult.No)
                {

                }


            }


        }  //SaveRoi

        private void button79_Click(object sender, EventArgs e)
        {
            try
            {
                if (CbDelete.Checked)
                {
                    DeletedRoi(Tx_RName);
                    AddToLog("Deleted", "ROI(s) Deleted.", 0, Color.Gray);
                }
                else
                {
                    int idx = dosyaSayisi + 1;

                    if (PRegions[idx] != null)
                    {

                        PRegions[idx].Dispose();
                        PRegions[idx] = null;
                        if (Cbk1.Checked)
                        {
                            HOperatorSet.DispObj(CurImage, hWin1.HalconWindow);
                        }
                        else
                        {
                            HOperatorSet.DispObj(CurImage2, hWin2.HalconWindow);
                        }

                        AddToLog("Deleted", "ROI(s) Deleted.", 0, Color.Gray);
                    }

                }
            }
            catch
            {

            }

        }  //DeleteRoi

        private void BtnClose_Click(object sender, EventArgs e)
        {

            tmrWenglor.Enabled = false;
            tmrWenglor.Stop();
            if (PlcBaglanti)
            {
                plc.Close();
            }
            Application.Exit();
        }

        private void BtnMin_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void Btn_Password(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == bir)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "1";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "1";
                }
            }
            else if (button == iki)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "2";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "2";
                }
            }
            else if (button == üc)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "3";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "3";
                }
            }

            else if (button == dort)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "4";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "4";
                }
            }
            else if (button == bes)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "5";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "5";
                }
            }
            else if (button == alti)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "6";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "6";
                }
            }
            else if (button == yedi)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "7";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "7";
                }
            }
            else if (button == sekiz)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "8";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "8";
                }
            }
            else if (button == dokuz)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "9";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "9";
                }
            }
            else if (button == button36)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "00";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "00";
                }
            }
            else if (button == sifir)
            {
                if (textBox16.Text == "")
                {
                    textBox16.Text = "0";
                }
                else
                {
                    textBox16.Text = textBox16.Text + "0";
                }
            }

        }

        private void Bt_Login_Click(object sender, EventArgs e)
        {
            if ("0000" == textBox16.Text)
            {
                login = true;
                LoginPasif(true);


                textBox16.Text = "";
                TabClear();
                this.TbAna.Parent = this.tabControl1; //show
                this.tbhomee.Parent = this.tabControl2; //show
            }
        }

        private void LoginPasif(bool v)
        {
            if (v)
            {
                login = true;
                BtPasword.BackColor = Color.Green;
            }
            else
            {
                login = false;
                BtPasword.BackColor = Color.Red;
            }
        }

        private void Bt_Clear_Click(object sender, EventArgs e)
        {
            textBox16.Text = "";
        }

        private void Bt_Exit_Click(object sender, EventArgs e)
        {
            login = false;
            LoginPasif(false);

        }

        ToolTip Controls_Tooltip(string baslik, string aciklama, Control cntrl)
        {
            ToolTip toolTip = new ToolTip();
            toolTip.Active = true; // Görünsün mü?
            toolTip.ToolTipTitle = baslik; // Çıkacak Mesajın Başlığı
            toolTip.ToolTipIcon = ToolTipIcon.Info; // Çıkacak Mesajda yer alacak ıkon
            toolTip.UseFading = true; // Silinerek kaybolma ve yavaşça görünme
            toolTip.UseAnimation = true; // Animasyonlu açılış
            toolTip.IsBalloon = true; // Baloncuk şekli mi dikdörtgen mi?
            toolTip.ShowAlways = true; // her zaman göster
            toolTip.AutoPopDelay = 3000; // Mesajın açık kalma süresi
            toolTip.ReshowDelay = 3000; // Mause çekildikten kaç ms sonra kaybolsun
            toolTip.InitialDelay = 1000; // Mesajın açılma süresi
            toolTip.BackColor = Color.Red; // arka plan rengi
            toolTip.ForeColor = Color.White; // yazı rengi
            toolTip.SetToolTip(cntrl, aciklama); // Hangi kontrolde görünsün
            return toolTip;
        }

        private void btn_Simulasyon_Click(object sender, EventArgs e)
        {


        }

        private void BtnSGrab_Click(object sender, EventArgs e)
        {
            //GrabImage(false);
        }

        private void hWin1_HMouseDown(object sender, HMouseEventArgs e)
        {
            HWindowControl hw = ((HWindowControl)sender);
        }

        private void Btn_Menu(object sender, EventArgs e)
        {
            Button button = sender as Button;
            if (button == BtnHome)
            {
                TabClear();
                this.TbAna.Parent = this.tabControl1; //show
                this.tbhomee.Parent = this.tabControl2; //show
            }
            else if (button == Btn_Resetle)
            {
                KayitliReceteResetle();
            }
            else if (button == Btn_DoCon)
            {
                try
                {
                    GrabNCheck(true);
                    //DoMeasure("ha");

                    DoMeasure(textBox13.Text);
                }
                catch (Exception Ex)
                {
                    AddToLog("Vision Err", Ex.ToString(), 5, Color.Red);
                }

            }
            else if (button == BtnRaporlar)
            {
                TabClear();
                this.tbreport.Parent = this.tabControl1; //show
            }
            else if (button == BtPasword)
            {
                TabClear();
                this.tbpasword.Parent = this.tabControl1; //show
            }
            else if (button == btInfo)
            {
                TabClear();
            
            }
            else if (button == BtnLog)
            {
                TabClear();
                this.tblog.Parent = this.tabControl1; //show
            }
            else if (button == BtnSettings)
            {
                if (login)
                {
                    TabClear();
                    this.TbAna.Parent = this.tabControl1; //show
                    this.tbsettings.Parent = this.tabControl2; //show
                }
                else
                {

                    DialogResult secenek = MessageBox.Show("Yönetici Girisi Yapmalısınız !", "Yönetici Girişi Aktif Değil", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                    if (secenek == DialogResult.Yes)
                    {
                        TabClear();
                        this.tbpasword.Parent = this.tabControl1; //show
                    }
                    else if (secenek == DialogResult.No)
                    {
                        TabClear();
                        this.TbAna.Parent = this.tabControl1; //show
                        this.tbsettings.Parent = this.tabControl2; //show
                    }

                }
            }

            else if (button == Bt_Recete)
            {
                if (login)
                {
                    tmrWenglor.Enabled = false;
                    tmrWenglor.Stop();
                    TabClear();
                    this.TbAna.Parent = this.tabControl1; //show
                    this.tbrecete.Parent = this.tabControl2; //show
                }
                else
                {

                    DialogResult secenek = MessageBox.Show("Yönetici Girisi Yapmalısınız !", "Yönetici Girişi Aktif Değil", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                    if (secenek == DialogResult.Yes)
                    {
                        TabClear();
                        this.tbpasword.Parent = this.tabControl1; //show
                    }
                    else if (secenek == DialogResult.No)
                    {
                        TabClear();
                        this.TbAna.Parent = this.tabControl1; //show
                        this.tbsettings.Parent = this.tabControl2; //show
                    }

                }
            }
            else if (button == BtnRemote)
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\AnyDesk.exe");
            }
            else if (button == Bt_key)
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\keyboard.exe");
            }
            else if (button == button45) // screen record
            {
                System.Diagnostics.Process.Start(Application.StartupPath + "\\Ekran Kaydet\\Ekran Kaydet.exe");
            }

        }

        private void button12_Click(object sender, EventArgs e)
        {
            TabClear();
            this.TbAna.Parent = this.tabControl1; //show
            this.tb_edit.Parent = this.tabControl2; //show
        }

        private void bT_GRAB_Click(object sender, EventArgs e)
        {

            GrabNCheck(true);
        }

        private void btgrab2_Click(object sender, EventArgs e)
        {
            //GrabImage(false);
        }

        private void bt_SaveSettings_Click(object sender, EventArgs e)
        {
            WriteConfigs();
        }

        private void bt_selectfolder1_Click(object sender, EventArgs e)
        {
            SelectedFile(txSource11);
        }

        private void bt_selectfolder2_Click(object sender, EventArgs e)
        {
            SelectedFile(txSource22);
        }

        private void bt_Roi2_Click(object sender, EventArgs e)
        {
            cb_Recete2.Text = "Seçiniz...";
            string rDir = Myyol + txt_edit.Text + "\\";
            ;
            try

            {
                dosyaSayisi = Directory.GetFiles(rDir, "*.hobj", SearchOption.AllDirectories).Length;
            }
            catch
            {
                dosyaSayisi = 0;
            }
            int idx = dosyaSayisi + 30;
            if (checkBox2.Checked)
            {
                PrintRegion(txt_edit, idx);
            }
            else
            {
                PrintRegion2(txt_edit, idx);
            }

        }

        private void bt_Roi_Delet_Click(object sender, EventArgs e)
        {
            try
            {
                if (CbDelete.Checked)
                {
                    DeletedRoi(txt_edit);
                    AddToLog("Deleted", "ROI(s) Deleted.", 0, Color.Gray);
                }
                else
                {
                    int idx = dosyaSayisi + 1;

                    if (PRegions[idx] != null)
                    {
                        PRegions[idx].Dispose();
                        PRegions[idx] = null;
                        HOperatorSet.DispObj(CurImage, hWin1.HalconWindow);
                        HOperatorSet.DispObj(CurImage2, hWin2.HalconWindow);
                        AddToLog("Deleted", "ROI(s) Deleted.", 0, Color.Gray);
                    }

                }
            }
            catch
            {

            }
        }

        private void bt_RoiSave2_Click(object sender, EventArgs e)
        {

        }

        private void listBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            Ksecilen = listBox1.SelectedItem.ToString();
            LoadWriteRegion();
            txt_edit.Text = Ksecilen;
        }

        private void button24_Click(object sender, EventArgs e)
        {
            KayitliReceteResetle();
        }

        private void bt_RoiSave2_Click_1(object sender, EventArgs e)
        {
            if (txt_edit.Text == "-")
            {
                MessageBox.Show("Reçete Adı Giriniz !", "Reçete Adı Giriniz ! ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (cb_Recete2.Text == "Seçiniz...")
            {
                MessageBox.Show("Görev Ataması Yapınız !", "Görev Ataması Yapınız ! ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {

                DialogResult secenek = MessageBox.Show("Seçilen ROI Bölgesi'ne " + txt_edit.Text.ToString() + " alan kontrolü kayıt edilsin mi", "BİLGİ", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                if (secenek == DialogResult.Yes)
                {
                    string rDir = Myyol + txt_edit.Text + "\\";

                    if (!Directory.Exists(rDir))
                        Directory.CreateDirectory(rDir);

                    int idx = dosyaSayisi + 30;
                    string MyValue = cb_Recete2.Text.ToString();
                    SaveRoiLabels2(rDir, MyValue, idx);
                }
                else if (secenek == DialogResult.No)
                {

                }



            }
        }

        private void Lbx_Hobj_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            try
            {
                string a = listBox1.SelectedItem.ToString();
                string b = Lbx_Hobj.SelectedItem.ToString();
                HOperatorSet.DispObj(CurImage, hWin1.HalconWindow);
                HOperatorSet.DispObj(CurImage2, hWin1.HalconWindow);
                string rDi = Myyol + a + "\\" + b;
                int satirno = Lbx_Hobj.SelectedIndex;
                Lbx_Labels.SelectedIndex = satirno;
                Lbx_Kamera.SelectedIndex = satirno;
                HOperatorSet.ReadRegion(out ho_RRecion, rDi);
                HOperatorSet.SetColor(hWin1.HalconWindow, "blue");
                HOperatorSet.DispObj(ho_RRecion, hWin1.HalconWindow);
                HOperatorSet.SetColor(hWin2.HalconWindow, "blue");
                HOperatorSet.DispObj(ho_RRecion, hWin2.HalconWindow);
            }
            catch
            {

            }
        }

        private void Lbx_Labels_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void bt_Temp_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        #endregion

        #region S7NET

        private void PlcBaglan()
        {

            try
            {
                CpuType cpu = (CpuType)Enum.Parse(typeof(CpuType), "S7300");
                plc = new Plc(cpu, "192.168.0.1", 0, 2);
                plc.Open();
                AddToLog("PLC", "Bağlandı", 6, Color.Green);
            }
            catch (Exception ex)
            {
                // button8.BackColor = Color.Red;
                // tmrWenglor.Enabled = true;
                AddToLog("PLC", "Bağlantı hatası" + ex.ToString(), 5, Color.Red);
                // MessageBox.Show("PLC BAĞLANTI HATASI");
            }

            if (plc.IsConnected)
            {
                PlcBaglanti = true;
                tmrWenglor.Enabled = true;
                button25.BackColor = Color.Green;
            }
            else
            {
                button25.BackColor = Color.Red;
                AddToLog("PLC", "Bağlantı hatası", 5, Color.Red);
                label86.Text = "PLC Bağlantı Hatası";

            }
        }

        private void PlcRead()
        {
            if (PlcBaglanti == true)
            {
                try
                {
                    Stopwatch watch = new Stopwatch();
                    watch.Start();
                    bool tetik = (bool)plc.Read("DB301.DBX14.0");
                    var encoder = ((uint)plc.Read("DB301.DBD0")).ConvertToFloat();
                    lbWarning.Text = encoder.ToString();

                    TimerSet(false); // tmr kapat

                    if (tetik == true)
                    {

                        T3 = true;
                        f = Convert.ToInt32(txt_En.Text);
                        se1 = Convert.ToInt32(textBox10.Text);
                        se2 = Convert.ToInt32(textBox11.Text);
                        R = f + encoder;
                        S1 = se1 + encoder;
                        S2 = se2 + encoder;
                        bT_GRAB.BackColor = Color.Green;
                        GrabImagea1(true);
                        label86.Text = "S7300 PLC'den Tetik Sinyali Alındı.. - 1";
                        var Doja = ((ushort)plc.Read("DB301.DBW12.0")).ToString();

                        lb_Recete.Text = "";

                    }

                    if (T2)
                    {
                        if (R <= encoder)
                        {
                            GrabImagea2(true);
                            T2 = false;
                            T4 = true;
                            label86.Text = "S7300 PLC'den Tetik Sinyali Alındı.. - 3";
                            var Doja = ((ushort)plc.Read("DB301.DBW12.0")).ToString();

                            lb_Recete.Text = Doja.ToString();
                        }
                    }

                    if (T3)
                    {
                        if (S1 <= encoder)
                        {
                            GrabImageu1(true);
                            T3 = false;
                            T2 = true;
                            label86.Text = "S7300 PLC'den Tetik Sinyali Alındı.. - 2";
                        }
                    }

                    if (T4)
                    {
                        if (S2 <= encoder)
                        {

                            GrabImage2(true);
                            SaveImage(String.Empty, lb_Recete.Text);
                            bT_GRAB.BackColor = myGriColor;
                            T4 = false;
                            label86.Text = "S7300 PLC'den Tetik Sinyali Bekleniyor...";

                            DoMeasure(lb_Recete.Text);

                            AddToLog("Vision Sys", "Kameralardan Görüntü Alındı", 6, Color.Green);
                            watch.Stop();
                            LblDTime.Text = watch.Elapsed.TotalSeconds.ToString();

                        }
                    }

                    TimerSet(true);  //tmr aç
                }
                catch (Exception EX)
                {
                    AddToLog("PLC", "Okuma Hatası" + EX.ToString(), 5, Color.Red);
                    //MessageBox.Show(EX.ToString());
                    TimerSet(true);
                }
            }
        }

        private void tmrWenglor_Tick(object sender, EventArgs e)
        {
            try
            {
                PlcRead();
            }
            catch (Exception EX)
            {
                AddToLog("Sensör", "Veri Gönderme Hatası", 6, Color.Red);
            }
        }

        private void TimerSet(bool Start)
        {
            if (Start)
            {
                tmrWenglor.Enabled = true;
                tmrWenglor.Start();
            }
            else
            {
                tmrWenglor.Enabled = false;
                tmrWenglor.Stop();
            }
        }
        #endregion

        #region RAPOR OLUŞTURMA

        private void SaveRapor(string sonuc, string ImageName)
        {
            string Tarih = DateTime.Now.ToString("yyyyMMdd");
            string dir = Application.StartupPath + "\\Settings\\Rapor\\" + Tarih;

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            String tarih = DateTime.Now.ToString("yyyyMMddHHmmss");

            string setFile = dir + "\\" + tarih + ".dat";
            DateTime saveTime = DateTime.Now;
            using (StreamWriter sw = new StreamWriter(setFile))
            {
                sw.WriteLine(saveTime.ToString("yyyy:MM:dd-HH:mm:ss") + ";" + ImageName + ";" + lb_Recete.Text + ";" + sonuc);
            }
            gunsonuyap();
        }

        private void ReadWriteRapor()
        {
            try
            {
                DateTime date = dateTimePicker3.Value;

                string path = Application.StartupPath + "\\Settings\\Rapor\\rapor\\rapor.dat";

                if (File.Exists(path))
                {
                    string[] lines = File.ReadAllLines(path);

                    try
                    {
                        int a = lines.Count();
                        if (lines.Length > 0)
                        {
                            for (int i = 0; i < a; i++)
                            {
                                string[] vals;
                                vals = lines[i].Split(';');
                                dataGridView4.Rows.Add(vals[0], vals[1], vals[2], vals[3]);
                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                AddToLog("ReadWriteRapor Err", ex.ToString(), 5, Color.Red);
            }

        }

        private void ReadRaporTotalOKNOK(out int ok, out int nok)
        {
            ok = 0;
            nok = 0;
            try
            {
                string toplam = "";
                for (int i = 0; i < dataGridView4.Rows.Count - 1; ++i)
                {
                    toplam = (dataGridView4.Rows[i].Cells[2].Value).ToString();
                    if (toplam == "OK")
                    {
                        ok++;
                    }
                    else
                    {
                        nok++;
                    }
                }

            }
            catch
            {

            }

        }

        private void button37_Click(object sender, EventArgs e)
        {
            try
            {
                string delet = Application.StartupPath + "\\Settings\\Rapor\\rapor\\rapor.dat";
                if (File.Exists(delet))
                {
                    File.Delete(delet);
                }
                // StreamWriter sw = new StreamWriter(delet, true);
                // sw.Close();
                String gun = "";
                String bas = "";
                String son = "";
                DateTime start = new DateTime();
                DateTime late = new DateTime();
                start = dateTimePicker3.Value;
                late = dateTimePicker4.Value;
                bas = start.ToString("yyyyMMdd");
                son = late.ToString("yyyyMMdd");
                //string ilk = Application.StartupPath + "\\Settings\\" + ilkmasa.Trim() + ".dat";
                gun = GunFarkikBul(start, late).ToString();

                string yeni = Application.StartupPath + "\\Settings\\Rapor\\rapor\\rapor.dat";

                for (int i = 0; i <= Convert.ToInt32(gun); i++)
                {

                    bas = start.AddDays(i).ToString("yyyyMMdd"); ;

                    string path = Application.StartupPath + "\\Settings\\Rapor\\" + bas + "\\rapor.dat";


                    FileMerge Birlestirici = new FileMerge(yeni);

                    Birlestirici.Load(path, yeni);

                    Birlestirici.SetEncoding(Encoding.UTF8);
                    bool BirlestiMi = Birlestirici.Save();

                    if (BirlestiMi)
                    {
                        //MessageBox.Show("");
                        //  File.Delete(ilk);
                        // Console.WriteLine("Dosyalar başarıyla birleştirildi: {0}", hedef);
                    }

                }

                ReadWriteRapor();
            }

            catch (Exception ex)
            {

            }
        } //rapor oluştur

        private void gunsonuyap()
        {
            try
            {

                String kat = "";
                DateTime dt = DateTime.Now;
                kat = dt.ToString("yyyyMMdd");
                //string ilk = Application.StartupPath + "\\Settings\\" + ilkmasa.Trim() + ".dat";

                string path = Application.StartupPath + "\\Settings\\Rapor\\" + kat + "\\";
                DirectoryInfo di = new DirectoryInfo(path);
                //FileInfo tipinden bir değişken oluşturuyoruz.
                //çünkü di.GetFiles methodu, bize FileInfo tipinden bir dizi dönüyor.
                FileInfo[] rgFiles = di.GetFiles();
                //foreach döngümüzle fgFiles içinde dönüyoruz.
                string rapor = path + "rapor.dat";
                if (File.Exists(rapor))
                {
                    File.Delete(rapor);
                }

                if (!File.Exists(rapor))
                {
                    int i = 0;

                    foreach (FileInfo fi in rgFiles)
                    {
                        i++;

                        string ilk = path + fi.ToString();

                        FileMerge Birlestirici = new FileMerge(rapor);

                        Birlestirici.Load(ilk, rapor);

                        Birlestirici.SetEncoding(Encoding.UTF8);
                        bool BirlestiMi = Birlestirici.Save();

                        if (BirlestiMi)
                        {
                            //MessageBox.Show("");
                            //  File.Delete(ilk);
                            // Console.WriteLine("Dosyalar başarıyla birleştirildi: {0}", hedef);
                        }

                    }

                 ;
                }

            }
            catch (Exception ex)
            {
                AddToLog("Rapor Err", ex.ToString(), 5, Color.Red);
            }
        }

        public int GunFarkikBul(DateTime dt1, DateTime dt2)
        {
            TimeSpan zama = new TimeSpan(); // zaman farkını bulmak adına kullanılacak olan nesne
            zama = dt1 - dt2;//metoda gelen 2 tarih arasındaki fark
            return Math.Abs(zama.Days); // 2 tarih arasındaki farkın kaç gün olduğu döndürülüyor.
        }

        private void button38_Click(object sender, EventArgs e)
        {
            pdfAktar.pdfKaydet(dataGridView4);
        } //rapor convert pdf

        #endregion

        #region FONKSİYONLAR

        private void OpenVisImage()
        {
            string myPath = @"C:\Images\" + textBox3.Text + @"\";
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = myPath;

        }

        private void SistemPerfomance()
        {
            //  cpuCounter = new PerformanceCounter("Process", "% Processor Time");
            ramCounter = new PerformanceCounter("Memory", "Available MBytes");

            lbRam.Text = getAvailableRAM();
            // lbCpu.Text = getCurrentCpuUsage();   
            DriveInfo info = new DriveInfo("C");

            long Total_Disk = info.TotalSize / (1048576 * 1024); //Mb cinsinden görmek için 1048576 böldük.
            long Available_Disk = info.TotalFreeSpace / (1048576 * 1024);
            long Full_Disk = Total_Disk - Available_Disk;
            string DiskDoluluk = "% " + (Convert.ToDouble(Full_Disk) / Convert.ToDouble(Total_Disk) * 100).ToString("#.0");
            LblDiskUyarı.Text = "Bilgisayar Hafızasının " + DiskDoluluk + " ' ı dolu. Lütfen Verilerinizi Yedekleyeniniz.";
            lbDisk.Text = Available_Disk.ToString();
        }

        public string getCurrentCpuUsage()
        {
            return cpuCounter.NextValue() + "%";
        }

        public string getAvailableRAM()
        {
            return ramCounter.NextValue() + "MB";
        }

        private void SetFontAndColors()
        {

            this.dataGridView4.DefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 15);
            this.dataGridView4.DefaultCellStyle.ForeColor = Color.Black;
            this.dataGridView4.DefaultCellStyle.BackColor = Color.White;
            this.dataGridView4.DefaultCellStyle.SelectionForeColor = Color.White;
            this.dataGridView4.DefaultCellStyle.SelectionBackColor = Color.Gray;
            this.dataGridView4.RowTemplate.Height = 40;

            this.dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Tahoma", 15);
            this.dataGridView1.DefaultCellStyle.ForeColor = Color.Black;
            this.dataGridView1.DefaultCellStyle.BackColor = Color.White;
            this.dataGridView1.DefaultCellStyle.SelectionForeColor = Color.White;
            this.dataGridView1.DefaultCellStyle.SelectionBackColor = Color.Gray;
            this.dataGridView1.RowTemplate.Height = 40;
        }

        private void SelectedFile(TextBox textBox)
        {
            try
            {
                filearray[0] = null;
                FolderBrowserDialog file = new FolderBrowserDialog();
                file.ShowDialog();

                string DosyaYolu = file.SelectedPath;
                string[] rgFiles = Directory.GetFiles(DosyaYolu);
                textBox.Text = DosyaYolu;

                AddToLog("Program", "Görüntüler Klasörden Yüklendi", 10, Color.Green);
            }
            catch
            {
                AddToLog("Program", "Görüntüler Klasörden Yüklenemedi", 10, Color.Green);
            }
        }  // Klasör seçme

        private void KayitliReceteResetle()
        {
            logCounter2++;
            listBox1.Items.Clear();
            LoadWriteRegion();
            List<string> klasorler = FolderFind(Myyol);
            foreach (string veri in klasorler)
            {
                listBox1.Items.Add(veri.ToString());

                logCounter2++;
                if (logCounter2 > 999) logCounter2 = 0;
            }

        }

        private void DoMeasure(string Recete)
        {
            try
            {
                string Rdir2 =Application.StartupPath + "\\Settings\\Araba\\"+Recete+".dat";
                if (File.Exists(Rdir2))
                {
                    string[] lines = File.ReadAllLines(Rdir2);

                    try
                    {
                        int a = lines.Count();
                        if (lines.Length > 0)
                        {
                            for (int i = 0; i < a; i++)
                            {
                                string[] vals;
                                vals = lines[i].Split(';');
                                ustsol = vals[0];
                                ustsag = vals[1];
                                altsol = vals[2];
                                altsag = vals[3];

                            }
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                }
                string rDir = Myyol + Recete + "\\";
                dosyaSayisi = Directory.GetFiles(rDir, "*.hobj", SearchOption.AllDirectories).Length;

                bool Result = true;
                for (int i = 0; i <= dosyaSayisi - 1; i++)
                {
                    LoadReceteControls(rDir, i);

                

                    if(ustsol=="0" && altsol=="0" && ustsag == "1" && altsag == "1")
                    {
                        HOperatorSet.AreaCenter(ho_RRecion, out HTuple area, out HTuple row, out HTuple column);
                        if (Convert.ToDouble(column.ToString()) >= 2048 && Convert.ToDouble(column.ToString()) <= 4096)
                        {

                            bool Rreturn = DoControl();
                            if (!Rreturn)
                            {
                                Result = false;
                            }

                        }
                    }

                    if (ustsol == "1" && altsol == "1" && ustsag == "0" && altsag == "0")
                    {
                        HOperatorSet.AreaCenter(ho_RRecion, out HTuple area, out HTuple row, out HTuple column);
                        if (Convert.ToDouble(column.ToString()) >= 0 && Convert.ToDouble(column.ToString()) <= 2048)
                        {

                            bool Rreturn = DoControl();
                            if (!Rreturn)
                            {
                                Result = false;
                            }

                        }
                    }


                    if (ustsol == "1" && altsol == "1" && ustsag == "1" && altsag == "1")
                    {
                        HOperatorSet.AreaCenter(ho_RRecion, out HTuple area, out HTuple row, out HTuple column);
                        if (Convert.ToDouble(column.ToString()) >= 0 && Convert.ToDouble(column.ToString()) <= 4096)
                        {

                            bool Rreturn = DoControl();
                            if (!Rreturn)
                            {
                                Result = false;
                            }

                        }
                    }



                }


      
                if (Result)
                {
                    LabelsResults("true");
                    AddToLog("Vision Result", "OK", 6, Color.Green);
                }
                else
                {
                    LabelsResults("false");
                    AddToLog("Vision Result", "NOK", 6, Color.Red);
                }

            }
            catch
            {

                AddToLog("Vision Err", "Error", 6, Color.Red);
            }

        }

        private void LoadIp()
        {

        }

        public List<string> FolderFind(string path)
        {
            List<string> klasor = new List<string>();
            DirectoryInfo di = new DirectoryInfo(path);
            DirectoryInfo[] getdir = di.GetDirectories();
            foreach (DirectoryInfo veri in getdir)
            {
                klasor.Add(veri.ToString());
            }
            return klasor;
        }

        private void LabelsResults(string result)
        {
            Lbltt.Text = (Convert.ToInt32(Lbltt.Text) + 1).ToString();

            if (result == "true")
            {
                LblSonuc.Text = "OK";
                LblSonuc.BackColor = Color.Green;
                lbOK.Text = (Convert.ToInt32(lbOK.Text) + 1).ToString();
                SaveRapor("OK", ImageName);
            }
            if (result == "false")
            {
                SaveImageNOK(String.Empty, lb_Recete.Text);
                LblSonuc.Text = "NOK";
                LblSonuc.BackColor = Color.Red;
                lbNOK.Text = (Convert.ToInt32(lbNOK.Text) + 1).ToString();
                SaveRapor("NOK", ImageName);
            }

            lbOKY.Text = "%" + (Convert.ToDouble(lbOK.Text) / Convert.ToDouble(Lbltt.Text) * 100).ToString("#.00");
            lbNOKY.Text = "%" + (Convert.ToDouble(lbNOK.Text) / Convert.ToDouble(Lbltt.Text) * 100).ToString("#.00");
        }

        private void LoadConfigs()
        {

            string setFile = Application.StartupPath + "\\Settings\\Config.dat";
            if (File.Exists(setFile))
            {
                string[] lines = File.ReadAllLines(setFile);
                if (lines.Length > 1)
                {
                    try
                    {
                        checkBox6.Checked = Convert.ToBoolean(lines[0]);
                        txSource11.Text = lines[1];
                        txSource22.Text = lines[2];
                        cbFormat1.Text = lines[3];
                        cbSaveOk.Checked = Convert.ToBoolean(lines[4]);
                        cbSaveNOk.Checked = Convert.ToBoolean(lines[5]);
                        cbSaveON.Checked = Convert.ToBoolean(lines[6]);
                        txt_En.Text = lines[7];
                        textBox10.Text = lines[8];
                        textBox11.Text = lines[9];
                    }
                    catch (Exception ex)
                    {
                        AddToLog("Settings", "Load Error. " + ex.Message, 6, Color.Red);
                    }
                }
                else AddToLog("Settings", "Configuration File not Valid. Save Settings.", 6, Color.Red);
            }
            else AddToLog("Settings", setFile + " Not Found! Configuration must be save.", 6, Color.Red);
        }

        private void WriteConfigs()
        {
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Settings\\Config.dat"))
            {
                sw.WriteLine(checkBox6.Checked.ToString());
                sw.WriteLine(txSource11.Text);
                sw.WriteLine(txSource22.Text);
                sw.WriteLine(cbFormat1.Text);
                sw.WriteLine(cbSaveOk.Checked.ToString());
                sw.WriteLine(cbSaveNOk.Checked.ToString());
                sw.WriteLine(cbSaveON.Checked.ToString());
                sw.WriteLine(txt_En.Text);
                sw.WriteLine(textBox10.Text);
                sw.WriteLine(textBox11.Text);

                MessageBox.Show("Ayarlar Kayıt Edildi", "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AddToLog("Save Config", "Configuration Saved Successfully", 9, Color.Blue);
            }
        }

        private void DeletedRoi(TextBox textBox)
        {
            try
            {
                string a = Lbx_Hobj.SelectedItem.ToString();

                string rDir = Myyol + textBox.Text + "\\";

                string prName = rDir + a;
                if (File.Exists(prName))
                    File.Delete(prName);



                string[] b = a.Split('.');
                string prNamer = rDir + b[0] + ".dat";
                if (File.Exists(prNamer))
                    File.Delete(prNamer);
                string prNamerd = rDir + b[0] + ".txt";
                if (File.Exists(prNamerd))
                    File.Delete(prNamerd);
                AddToLog("Deleted", "ROI(s) Deleted.", 0, Color.Gray);
            }
            catch (Exception ex)
            {
                AddToLog("DeletedRoi Err", ex.ToString(), 5, Color.Red);
            }

        }

        private void LoadReceteControls(string Recete, int idx)
        {
     
            DirectoryInfo di = new DirectoryInfo(Recete);
            FileInfo[] files = di.GetFiles("*.dat");

            string text = Recete + files[idx].Name;

            //string setFile = Recete + "P"+idx.ToString() + ".dat";

            if (File.Exists(text))
            {
                string[] lines = File.ReadAllLines(text);
                if (lines.Length > 0)
                {
                    try
                    {

                        V_control = lines[0];

                    }
                    catch (Exception ex)
                    {

                    }
                }


            }

            FileInfo[] files2 = di.GetFiles("*.txt");

            string text2 = Recete + files2[idx].Name;

            //string setFile = Recete + "P"+idx.ToString() + ".dat";

            if (File.Exists(text2))
            {
                string[] liness = File.ReadAllLines(text2);
                if (liness.Length > 0)
                {
                    try
                    {

                        Kamera = liness[0];

                    }
                    catch (Exception ex)
                    {

                    }
                }


            }

            //string rDir = Recete + "P"+idx.ToString()+".hobj";
            //string[] filePaths = Directory.GetFiles(rDir, "*.hobj", SearchOption.TopDirectoryOnly);


            DirectoryInfo dir = new DirectoryInfo(Recete);
            FileInfo[] filess = di.GetFiles("*.hobj");
            string region = Recete + filess[idx].Name;
            HOperatorSet.ReadRegion(out ho_RRecion, region);
    
    
        }

        private void LoadWriteRegion()
        {
            try
            {
                Lbx_Hobj.Items.Clear();
                Lbx_Labels.Items.Clear();
                Lbx_Kamera.Items.Clear();
                string setFile = Myyol + Ksecilen + "\\";
                DirectoryInfo d1 = new DirectoryInfo(setFile);
                Files = d1.GetFiles("*.dat");

                foreach (FileInfo file in Files)
                {
                    string[] lines = File.ReadAllLines(setFile + file);
                    if (lines.Length > 0)
                    {

                        Lbx_Labels.Items.Add(lines[0]);
                    }
                }

                Files = d1.GetFiles("*.txt");

                foreach (FileInfo file2 in Files)
                {
                    string[] lines = File.ReadAllLines(setFile + file2);
                    if (lines.Length > 0)
                    {

                        Lbx_Kamera.Items.Add(lines[0]);
                    }
                }


                DirectoryInfo d = new DirectoryInfo(setFile);
                Files = d.GetFiles("*.hobj");

                foreach (FileInfo file in Files)
                {
                    Lbx_Hobj.Items.Add(file.Name);
                }
            }
            catch
            { }
        }

        private ListViewItem AddToLog(string grp, string dtl, int imgidx, Color clr)
        {
            if (logCounter >= 1000) logCounter = 0;

            string logDir = Application.StartupPath + "\\Logs\\" + DateTime.Now.ToString("yyyyMMdd");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            string logLine = grp + "\t" + dtl;

            if (imgidx == 5)
            {
                File.AppendAllText(logDir + "\\Log.txt", DateTime.Now.ToString("HH:mm:ss.fff") + " " + logLine + Environment.NewLine);

            }

            int maxCap = 64;
            if (listView2.Items.Count > maxCap) listView2.Items.Clear();

            ListViewItem listItem = new ListViewItem(logCounter.ToString());
            listItem.ImageIndex = imgidx;
            listItem.SubItems.Add(DateTime.Now.ToString("HH:mm:ss.fff"));
            listItem.SubItems.Add(grp);
            listItem.SubItems.Add(dtl);
            listView2.Items.Add(listItem);

            listView2.Items[listView2.Items.Count - 1].ForeColor = clr;
            listView2.Refresh();
            logCounter++;
            if (logCounter > 999) logCounter = 0;

            listView2.Items[listView2.Items.Count - 1].EnsureVisible();

            return listItem;
        }

        private void LoadSettings()
        {
            string fName = Application.StartupPath + "\\Settings\\Sets.dat";
            if (File.Exists(fName))
            {
                string[] lines = File.ReadAllLines(fName);
                try
                {

                }
                catch (Exception ex)
                {
                    AddToLog("Load Set Err", ex.Message, 6, Color.Red);
                }
            }
        }

        private void SaveSettings()
        {
            using (StreamWriter sw = new StreamWriter(Application.StartupPath + "\\Settings\\Sets.dat"))
            {

            }
        }

        #endregion

        #region HALCON

        private bool InitHALCON()
        {
            bool retVal = false;
            AddToLog("Image Processing", "Image Processing Engine Initializing...", 0, Color.Gray);

            string hFile = Application.StartupPath + "\\Settings\\LineX.hdev";
            if (File.Exists(hFile))
            {
                try
                {
                    hDevEngine.SetEngineAttribute("execute_procedures_jit_compiled", "true");
                    hDevEngine.SetProcedurePath(Application.StartupPath + "\\Settings");
                    HDevProgram hDevProgram = new HDevProgram(hFile);

                    HDevProcedure hDevProcOpenGrabber = new HDevProcedure(hDevProgram, "OpenGrabber");
                    HDevProcedure hDevProcOpenGrabber2 = new HDevProcedure(hDevProgram, "OpenGrabber2");
                    HDevProcedure hDevProcGrabImagea1 = new HDevProcedure(hDevProgram, "GrabImagea1");
                    HDevProcedure hDevProcGrabImagea2 = new HDevProcedure(hDevProgram, "GrabImagea2");
                    HDevProcedure hDevProcGrabImageu1 = new HDevProcedure(hDevProgram, "GrabImageu1");
                    HDevProcedure hDevProcGrabImage2 = new HDevProcedure(hDevProgram, "GrabImage2");
                    // HDevProcedure hDevProcPTiledImages = new HDevProcedure(hDevProgram, "PTiledImages");
                    HDevProcedure hDevProcDoControls = new HDevProcedure(hDevProgram, "DoControl");

                    HDevProcedure hDevProchReadModels = new HDevProcedure(hDevProgram, "ReadModels");
                    // HDevProcedure hDevProcLoadRegions = new HDevProcedure(hDevProgram, "LoadRegions");

                    hOpenGrabber1 = new HDevProcedureCall(hDevProcOpenGrabber);
                    hOpenGrabber2 = new HDevProcedureCall(hDevProcOpenGrabber2);
                    hGrabImagea1 = new HDevProcedureCall(hDevProcGrabImagea1);
                    hGrabImagea2 = new HDevProcedureCall(hDevProcGrabImagea2);
                    hGrabImageu1 = new HDevProcedureCall(hDevProcGrabImageu1);
                    hGrabImage2 = new HDevProcedureCall(hDevProcGrabImage2);
                    hGrabImage2 = new HDevProcedureCall(hDevProcGrabImage2);
                    // hGrabImage.SetInputCtrlParamTuple("WinHandle1", hWin1.HalconWindow);

                    hReadModels = new HDevProcedureCall(hDevProchReadModels);
                    hDoControls = new HDevProcedureCall(hDevProcDoControls);
                    hGrabImage2.SetInputCtrlParamTuple("WinHandle1", hWin1.HalconWindow);
                    hGrabImage2.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);
                    //   hOpenGrabber1.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);

                    AddToLog("Script", "Image Processing Script Loaded and Compiled.", 1, Color.Green);

                    //    hLoadRegions = new HDevProcedureCall(hDevProcLoadRegions);

                    HOperatorSet.SetDraw(hWin1.HalconWindow, "margin");
                    HOperatorSet.SetDraw(hWin2.HalconWindow, "margin");
                    retVal = true;

                }
                catch (Exception ex)
                {
                    AddToLog("Init Err", ex.Message, 5, Color.Red);
                }
            }
            else
            {
                AddToLog("Init Err", "Image Processing Script File Not Found!", 5, Color.Red);
            }
            return retVal;
        }

        private void ReadModels()
        {
            try
            {


                hReadModels.Execute();
                hv_Model1 = hReadModels.GetOutputCtrlParamTuple("ModelIds1");
                hv_Model2 = hReadModels.GetOutputCtrlParamTuple("ModelIds2");
                hv_Model3 = hReadModels.GetOutputCtrlParamTuple("MaviModels");
                hv_Model4 = hReadModels.GetOutputCtrlParamTuple("YesilModels");

            }
            catch (Exception ex)
            {
                AddToLog("CreatE Models", ex.Message, 5, Color.Red);
            }
        }

        private bool InitCameras()
        {
            bool retVal = false;
            try
            {
                string camInterface = checkBox6.Checked ? "GigeVision2" : "File";
                // string camInterface = "File";
                hOpenGrabber1.SetInputCtrlParamTuple("Interface1", camInterface);
                // hOpenGrabber1.SetInputCtrlParamTuple("Interface2", camInterface);

                hOpenGrabber1.SetInputCtrlParamTuple("Source1", txSource11.Text);
                // hOpenGrabber1.SetInputCtrlParamTuple("Source2", txSource22.Text);

                AddToLog("Cam Init", "Print Inspection Camera (" + camInterface + ") Initializing...", 0, Color.Gray);

                hOpenGrabber1.SetInputCtrlParamTuple("WinHandle1", hWin1.HalconWindow);
                // hOpenGrabber1.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);

                hOpenGrabber1.Execute();
                hv_AcqHandle1 = hOpenGrabber1.GetOutputCtrlParamTuple("AcqHandle_1");
                // hv_AcqHandle2 = hOpenGrabber1.GetOutputCtrlParamTuple("AcqHandle_2");

                AddToLog("Cam Ok 1", "Camera  Succesfully Opened", 6, Color.Green);

                //      HTuple hv_Params1 = hOpenGrabber1.GetOutputCtrlParamTuple("Params1");
                //    StartExposure1 = hv_Params1[0];
                //    AddToLog("Camera", string.Format("Camera Successfully Opened. ExposureTime:{0}, Gain:{1} FPS:{2} Device:{3}", StartExposure1.D, hv_Params1[1].D, hv_Params1[2].D, hv_Params1[3].S), 10, Color.Black);

                retVal = true;
            }
            catch (Exception ex)
            {
                AddToLog("Cam Err 1", ex.Message, 5, Color.Red);
            }
            return retVal;
        }

        private bool InitCameras2()
        {
            bool retVal = false;
            try
            {
                string camInterface = checkBox6.Checked ? "GigeVision2" : "File";
                //  string camInterface = "File";
                hOpenGrabber2.SetInputCtrlParamTuple("Interface2", camInterface);
                //   hOpenGrabber1.SetInputCtrlParamTuple("Interface2", camInterface);


                hOpenGrabber2.SetInputCtrlParamTuple("Source2", txSource22.Text);
                //  hOpenGrabber1.SetInputCtrlParamTuple("Source2", txSource22.Text);


                AddToLog("Cam Init", "Print Inspection Camera (" + camInterface + ") Initializing...", 0, Color.Gray);

                hOpenGrabber2.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);
                //     hOpenGrabber1.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);

                hOpenGrabber2.Execute();
                hv_AcqHandle2 = hOpenGrabber2.GetOutputCtrlParamTuple("AcqHandle_2");
                //  hv_AcqHandle2 = hOpenGrabber1.GetOutputCtrlParamTuple("AcqHandle_2");

                AddToLog("Cam Ok 2", "Camera  Succesfully Opened", 6, Color.Green);
                // P1 = hOpenGrabber2.GetOutputCtrlParamTuple("Params2");
                // String C1 = P1.ToString();
                //  StartExposure1 = hv_Params1[0];
                //   AddToLog("Camera", string.Format("Camera Successfully Opened. ExposureTime:{0}, Gain:{1} FPS:{2} Device:{3}", StartExposure1.D, hv_Params1[1].D, hv_Params1[2].D, hv_Params1[3].S), 10, Color.Black);

                retVal = true;
            }
            catch (Exception ex)
            {
                AddToLog("Cam Err 2", ex.Message, 5, Color.Red);
            }
            return retVal;
        }

        private bool GrabImagea1(bool async)
        {
            bool retVal = false;
            try
            {

                hGrabImagea1.SetInputCtrlParamTuple("AcqHandle_1", hv_AcqHandle1);
                hGrabImagea1.Execute();
                retVal = true;
                ho_Imagea1 = hGrabImagea1.GetOutputIconicParamImage("Imagea1");
            }
            catch (Exception ex)
            {
                AddToLog("Grab Image Hata", ex.Message, 5, Color.Red);
            }
            return retVal;
        }

        private bool GrabImagea2(bool async)
        {
            bool retVal = false;
            try
            {
                hGrabImagea2.SetInputCtrlParamTuple("AcqHandle_1", hv_AcqHandle1);
                hGrabImagea2.Execute();
                retVal = true;
                ho_Imagea2 = hGrabImagea2.GetOutputIconicParamImage("Imagea2");


            }
            catch (Exception ex)
            {
                AddToLog("Grab Image Hata", ex.Message, 5, Color.Red);
            }
            return retVal;
        }

        private bool GrabImageu1(bool async)
        {
            bool retVal = false;
            try
            {

                hGrabImageu1.SetInputCtrlParamTuple("AcqHandle_2", hv_AcqHandle2);
                hGrabImageu1.Execute();
                retVal = true;
                ho_Imageu1 = hGrabImageu1.GetOutputIconicParamImage("Imageu1");


            }
            catch (Exception ex)
            {
                AddToLog("Grab Image Hata", ex.Message, 5, Color.Red);
            }
            return retVal;
        }

        private bool GrabImage2(bool async)
        {
            bool retVal = false;
            try
            {
                string camInterface = checkBox6.Checked ? "GigeVision2" : "File";
                //  string camInterface = "File";
                hGrabImage2.SetInputCtrlParamTuple("Interface1", camInterface);

                hGrabImage2.SetInputIconicParamObject("Imagea1", ho_Imagea1);
                hGrabImage2.SetInputIconicParamObject("Imagea2", ho_Imagea2);
                hGrabImage2.SetInputIconicParamObject("Imageu1", ho_Imageu1);
                hGrabImage2.SetInputCtrlParamTuple("AcqHandle_1", hv_AcqHandle1);
                hGrabImage2.SetInputCtrlParamTuple("AcqHandle_2", hv_AcqHandle2);

                hGrabImage2.SetInputCtrlParamTuple("WinHandle1", hWin1.HalconWindow);
                hGrabImage2.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);

                hGrabImage2.Execute();
                //GrabTimes[iPos] = DateTime.Now;

                retVal = true;

                ho_Tile1 = hGrabImage2.GetOutputIconicParamImage("TiledImage1");
                ho_Tile2 = hGrabImage2.GetOutputIconicParamImage("TiledImage2");
                //  ho_Image2 = hGrabImage.GetOutputIconicParamImage("Image2");

                AddToLog("Image Processing", "Görüntü Alındı", 0, Color.Gray);

            }
            catch (Exception ex)
            {
                AddToLog("Grab Image 2 Hata", ex.Message, 5, Color.Red);
            }
            return retVal;
        }

        private bool GrabNCheck(bool async)
        {
            bool retVal = false;

            GrabImagea1(async);
            GrabImagea2(async);
            GrabImageu1(async);
            GrabImage2(async);
            return retVal;
        }

        private bool DoControl()
        {
            bool camsOK = false;

            try
            {
                hDoControls.SetInputCtrlParamTuple("WinHandle1", hWin1.HalconWindow);
                hDoControls.SetInputCtrlParamTuple("WinHandle2", hWin2.HalconWindow);
                hDoControls.SetInputCtrlParamTuple("Kontrol", V_control);
                hDoControls.SetInputCtrlParamTuple("Kamera", Kamera);

                hDoControls.SetInputCtrlParamTuple("ModelIds1", hv_Model1);
                hDoControls.SetInputCtrlParamTuple("ModelIds2", hv_Model2);

                hDoControls.SetInputCtrlParamTuple("MaviModels", hv_Model3);
                hDoControls.SetInputCtrlParamTuple("YesilModels", hv_Model4);

                hDoControls.SetInputIconicParamObject("TiledImage1", ho_Tile1);
                hDoControls.SetInputIconicParamObject("TiledImage2", ho_Tile2);
                hDoControls.SetInputIconicParamObject("Recion", ho_RRecion);
                hDoControls.Execute();

                HTuple hv_Result = hDoControls.GetOutputCtrlParamTuple("Result");

                camsOK = hv_Result.I == 1;
                AddToLog("Image Processing", "Kontrol Yapıldı", 0, Color.Gray);
            }
            catch (Exception ex)
            {
                AddToLog("Vision Err", ex.Message, 5, Color.Red);
            }

            return camsOK;
        }

        private void SaveRoiLabels(string rDir, string recete, int idx)
        {
            try
            {

                string prName = rDir + "P" + idx.ToString() + ".hobj";
                string prNameLabel = rDir + "P" + idx.ToString() + ".dat";
                string prNameLabel2 = rDir + "P" + idx.ToString() + ".txt";
                System.Threading.Thread.Sleep(100);
                if (PRegions[idx] == null)
                {
                    File.Delete(prName);
                }
                else
                {
                    HOperatorSet.WriteRegion(PRegions[idx], prName);

                    using (StreamWriter sw = new StreamWriter(prNameLabel))
                    {
                        sw.WriteLine(recete);
                    }

                    using (StreamWriter sw = new StreamWriter(prNameLabel2))
                    {
                        if (Cbk1.Checked)
                        {
                            sw.WriteLine("1");
                        }
                        else
                        {
                            sw.WriteLine("2");
                        }

                    }
                    System.Threading.Thread.Sleep(500);
                    MessageBox.Show("Kayıt Edildi ", "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {

                AddToLog("SaveRoiLabels Err", ex.ToString(), 5, Color.Red);
            }

        }  //log ekle

        private void SaveRoiLabels2(string rDir, string recete, int idx)
        {
            try
            {

                string prName = rDir + "P" + idx.ToString() + ".hobj";
                string prNameLabel = rDir + "P" + idx.ToString() + ".dat";
                string prNameLabel2 = rDir + "P" + idx.ToString() + ".txt";
                System.Threading.Thread.Sleep(100);
                if (PRegions[idx] == null)
                {
                    File.Delete(prName);
                }
                else
                {
                    HOperatorSet.WriteRegion(PRegions[idx], prName);

                    using (StreamWriter sw = new StreamWriter(prNameLabel))
                    {
                        sw.WriteLine(recete);
                    }

                    using (StreamWriter sw = new StreamWriter(prNameLabel2))
                    {
                        if (checkBox2.Checked)
                        {
                            sw.WriteLine("1");
                        }
                        else
                        {
                            sw.WriteLine("2");
                        }

                    }
                    System.Threading.Thread.Sleep(500);
                    MessageBox.Show("Kayıt Edildi ", "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {

                AddToLog("SaveRoiLabels Err", ex.ToString(), 5, Color.Red);
            }

        }  //log ekle

        private void SaveImage(string folder, string ReceteFolder)
        {

            try
            {
                if (!SystemReady) return;

                DateTime saveTime = DateTime.Now;

                string pType = cbFormat.Text;
                string ext = pType;
                if (cbFormat.SelectedIndex > 2)
                {
                    ext = "jpeg";
                }

                string dir = Application.StartupPath + "\\Images\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + ReceteFolder;
                if (!folder.Equals(string.Empty)) dir = dir + "\\" + folder;

                string dir1 = dir + "\\Cam1";
                string dir2 = dir + "\\Cam2";

                if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
                if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

                string ImageNamed = saveTime.ToString("yyyyMMddHHmmss_fff");
                ImageName = ImageNamed + "." + ext;
                string pName1 = dir1 + "\\" + ImageNamed + "." + ext;
                HOperatorSet.WriteImage(ho_Tile1, pType, 0, pName1);

                string pName2 = dir2 + "\\" + ImageNamed + "." + ext;
                HOperatorSet.WriteImage(ho_Tile2, pType, 0, pName2);

                int ms = (int)DateTime.Now.Subtract(saveTime).TotalMilliseconds;

                AddToLog("Saved Image", "Grabbed Image Saved to " + dir + " folder. (" + ms.ToString() + " ms.)", 9, Color.Gray);
            }
            catch (Exception ex)
            {
                AddToLog("Save Image Err", ex.ToString(), 5, Color.Red);
            }

        }

        private void SaveImageNOK(string folder, string ReceteFolder)
        {

            try
            {
                if (!SystemReady) return;

                DateTime saveTime = DateTime.Now;

                string pType = cbFormat.Text;
                string ext = pType;
                if (cbFormat.SelectedIndex > 2)
                {
                    ext = "jpeg";
                }

                string dir = Application.StartupPath + "\\Images\\NOK\\" + DateTime.Now.ToString("yyyyMMdd") + "\\" + ReceteFolder;
                if (!folder.Equals(string.Empty)) dir = dir + "\\" + folder;

                string dir1 = dir + "\\Cam1";
                string dir2 = dir + "\\Cam2";

                if (!Directory.Exists(dir1)) Directory.CreateDirectory(dir1);
                if (!Directory.Exists(dir2)) Directory.CreateDirectory(dir2);

                string ImageNamed = saveTime.ToString("yyyyMMddHHmmss_fff");
                ImageName = ImageNamed + "." + ext;
                string pName1 = dir1 + "\\" + ImageNamed + "." + ext;
                HOperatorSet.WriteImage(ho_Tile1, pType, 0, pName1);

                string pName2 = dir2 + "\\" + ImageNamed + "." + ext;
                HOperatorSet.WriteImage(ho_Tile2, pType, 0, pName2);

                int ms = (int)DateTime.Now.Subtract(saveTime).TotalMilliseconds;

                AddToLog("Saved Image", "Grabbed Image Saved to " + dir + " folder. (" + ms.ToString() + " ms.)", 9, Color.Gray);
            }
            catch (Exception ex)
            {
                AddToLog("Save Image Err", ex.ToString(), 5, Color.Red);
            }

        }

        private HObject CurImage
        {
            get
            {
                return ho_Tile1;

            }
        }

        private HObject CurImage2
        {
            get
            {
                return ho_Tile2;

            }
        }

        private void PrintRegion(TextBox textBox, int idx)
        {
            HWindowControl hw = hWin1;
            HOperatorSet.SetColored(hw.HalconWindow, 12);

            HOperatorSet.DrawRectangle1(hw.HalconWindow, out HTuple r1, out HTuple c1, out HTuple r2, out HTuple c2);
            HRegion hoRect = new HRegion();
            hoRect.GenRectangle1(r1, c1, r2, c2);



            //  int idx= dosyaSayisi+1;
            PRegions[idx] = hoRect;

            HRegion hoReg = PRegions[idx];
            if (hoReg == null)
            {
                PRegions[idx] = hoRect;
                HOperatorSet.DispObj(CurImage, hw.HalconWindow);
                HOperatorSet.SetColor(hw.HalconWindow, "blue");
                HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);

            }
            else
            {
                HOperatorSet.DispObj(CurImage, hw.HalconWindow);
                HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);
                //if (bRem.BackColor == Color.LightGreen)
                //{
                //    HRegion ho_Diff = new HRegion();
                //    ho_Diff = hoReg.Difference(hoRect);
                //    PRegions[idx] = ho_Diff;
                //    HOperatorSet.DispObj(CurImage, hw.HalconWindow);
                //    HOperatorSet.SetColor(hw.HalconWindow, "blue");
                //    HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);
                //}
                //else
                //{
                HRegion ho_Union = new HRegion();
                ho_Union = hoReg.Union2(hoRect);
                PRegions[idx] = ho_Union;
                HOperatorSet.DispObj(CurImage, hw.HalconWindow);
                HOperatorSet.SetColor(hw.HalconWindow, "blue");
                HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);
                //}
            }


        }

        private void PrintRegion2(TextBox textBox, int idx)
        {
            HWindowControl hw = hWin2;
            HOperatorSet.SetColored(hw.HalconWindow, 12);

            HOperatorSet.DrawRectangle1(hw.HalconWindow, out HTuple r1, out HTuple c1, out HTuple r2, out HTuple c2);
            HRegion hoRect = new HRegion();
            hoRect.GenRectangle1(r1, c1, r2, c2);

            string rDir = Myyol + textBox.Text + "\\";
            ;
            try

            {
                dosyaSayisi = Directory.GetFiles(rDir, "*.hobj", SearchOption.AllDirectories).Length;
            }
            catch
            {
                dosyaSayisi = 0;
            }

            PRegions[idx] = hoRect;

            HRegion hoReg = PRegions[idx];
            if (hoReg == null)
            {
                PRegions[idx] = hoRect;
                HOperatorSet.DispObj(CurImage2, hw.HalconWindow);
                HOperatorSet.SetColor(hw.HalconWindow, "blue");
                HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);

            }
            else
            {
                HOperatorSet.DispObj(CurImage2, hw.HalconWindow);
                HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);
                //if (bRem.BackColor == Color.LightGreen)
                //{
                //    HRegion ho_Diff = new HRegion();
                //    ho_Diff = hoReg.Difference(hoRect);
                //    PRegions[idx] = ho_Diff;
                //    HOperatorSet.DispObj(CurImage, hw.HalconWindow);
                //    HOperatorSet.SetColor(hw.HalconWindow, "blue");
                //    HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);
                //}
                //else
                //{
                HRegion ho_Union = new HRegion();
                ho_Union = hoReg.Union2(hoRect);
                PRegions[idx] = ho_Union;
                HOperatorSet.DispObj(CurImage2, hw.HalconWindow);
                HOperatorSet.SetColor(hw.HalconWindow, "blue");
                HOperatorSet.DispObj(PRegions[idx], hw.HalconWindow);
                //}
            }


        }

        #endregion

        #region HALCONDRAWRECT

        public void DispRects(HObject ho_Image, out HObject ho_Rects, HTuple hv_WinHandle)
        {




            // Stack for temporary objects 
            HObject[] OTemp = new HObject[20];

            // Local iconic variables 

            HObject ho_R0, ho_R1, ho_R2, ho_R3, ho_R4;
            HObject ho_R5, ho_R6, ho_R7, ho_R8, ho_R9, ho_R10, ho_R11;
            HObject ho_R12, ho_R13, ho_R14, ho_R15, ho_R16, ho_R17;
            HObject ho_R18, ho_R19, ho_R20, ho_R21, ho_R22, ho_R23;
            HObject ho_R24, ho_R25, ho_R26;

            // Local control variables 

            HTuple hv_Width = null, hv_Height = null;
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rects);
            HOperatorSet.GenEmptyObj(out ho_R0);
            HOperatorSet.GenEmptyObj(out ho_R1);
            HOperatorSet.GenEmptyObj(out ho_R2);
            HOperatorSet.GenEmptyObj(out ho_R3);
            HOperatorSet.GenEmptyObj(out ho_R4);
            HOperatorSet.GenEmptyObj(out ho_R5);
            HOperatorSet.GenEmptyObj(out ho_R6);
            HOperatorSet.GenEmptyObj(out ho_R7);
            HOperatorSet.GenEmptyObj(out ho_R8);
            HOperatorSet.GenEmptyObj(out ho_R9);
            HOperatorSet.GenEmptyObj(out ho_R10);
            HOperatorSet.GenEmptyObj(out ho_R11);
            HOperatorSet.GenEmptyObj(out ho_R12);
            HOperatorSet.GenEmptyObj(out ho_R13);
            HOperatorSet.GenEmptyObj(out ho_R14);
            HOperatorSet.GenEmptyObj(out ho_R15);
            HOperatorSet.GenEmptyObj(out ho_R16);
            HOperatorSet.GenEmptyObj(out ho_R17);
            HOperatorSet.GenEmptyObj(out ho_R18);
            HOperatorSet.GenEmptyObj(out ho_R19);
            HOperatorSet.GenEmptyObj(out ho_R20);
            HOperatorSet.GenEmptyObj(out ho_R21);
            HOperatorSet.GenEmptyObj(out ho_R22);
            HOperatorSet.GenEmptyObj(out ho_R23);
            HOperatorSet.GenEmptyObj(out ho_R24);
            HOperatorSet.GenEmptyObj(out ho_R25);
            HOperatorSet.GenEmptyObj(out ho_R26);
            try
            {
                HOperatorSet.SetDraw(hv_WinHandle, "margin");
                HOperatorSet.GetImageSize(ho_Image, out hv_Width, out hv_Height);

                ho_R0.Dispose();
                HOperatorSet.GenRectangle1(out ho_R0, 20, 410, 55, 470);
                ho_R1.Dispose();
                HOperatorSet.MoveRegion(ho_R0, out ho_R1, 0, 50);
                ho_R2.Dispose();
                HOperatorSet.MoveRegion(ho_R1, out ho_R2, 26, 0);
                ho_R3.Dispose();
                HOperatorSet.MoveRegion(ho_R2, out ho_R3, 26, 0);
                ho_R4.Dispose();
                HOperatorSet.MoveRegion(ho_R3, out ho_R4, 26, 0);
                ho_R5.Dispose();
                HOperatorSet.MoveRegion(ho_R4, out ho_R5, 26, 0);
                ho_R6.Dispose();
                HOperatorSet.MoveRegion(ho_R5, out ho_R6, 26, 0);
                ho_R7.Dispose();
                HOperatorSet.MoveRegion(ho_R6, out ho_R7, 26, 0);
                ho_R8.Dispose();
                HOperatorSet.MoveRegion(ho_R7, out ho_R8, 26, 0);
                ho_R9.Dispose();
                HOperatorSet.MoveRegion(ho_R8, out ho_R9, 26, 0);

                ho_R10.Dispose();
                HOperatorSet.MoveRegion(ho_R9, out ho_R10, 0, -50);
                ho_R11.Dispose();
                HOperatorSet.MoveRegion(ho_R10, out ho_R11, 0, -50);
                ho_R12.Dispose();
                HOperatorSet.MoveRegion(ho_R11, out ho_R12, 0, -50);
                ho_R13.Dispose();
                HOperatorSet.MoveRegion(ho_R12, out ho_R13, 0, -50);
                ho_R14.Dispose();
                HOperatorSet.MoveRegion(ho_R13, out ho_R14, 0, -50);
                ho_R15.Dispose();
                HOperatorSet.MoveRegion(ho_R14, out ho_R15, 0, -50);
                ho_R16.Dispose();
                HOperatorSet.MoveRegion(ho_R15, out ho_R16, 0, -50);
                ho_R17.Dispose();
                HOperatorSet.MoveRegion(ho_R16, out ho_R17, 0, -50);
                ho_R18.Dispose();
                HOperatorSet.MoveRegion(ho_R17, out ho_R18, 0, -50);

                ho_R19.Dispose();
                HOperatorSet.MoveRegion(ho_R18, out ho_R19, -26, 0);
                ho_R20.Dispose();
                HOperatorSet.MoveRegion(ho_R19, out ho_R20, -26, 0);
                ho_R21.Dispose();
                HOperatorSet.MoveRegion(ho_R20, out ho_R21, -26, 0);
                ho_R22.Dispose();
                HOperatorSet.MoveRegion(ho_R21, out ho_R22, -26, 0);
                ho_R23.Dispose();
                HOperatorSet.MoveRegion(ho_R22, out ho_R23, -26, 0);
                ho_R24.Dispose();
                HOperatorSet.MoveRegion(ho_R23, out ho_R24, -26, 0);
                ho_R25.Dispose();
                HOperatorSet.MoveRegion(ho_R24, out ho_R25, -26, 0);
                ho_R26.Dispose();
                HOperatorSet.MoveRegion(ho_R25, out ho_R26, -26, 30);

                ho_Rects.Dispose();
                HOperatorSet.ConcatObj(ho_R0, ho_R1, out ho_Rects);
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R2, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R3, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R4, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R5, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R6, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R7, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R8, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R9, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R10, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R11, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R12, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R13, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R14, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R15, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R16, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R17, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R18, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R19, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R20, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R21, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R22, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R23, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R24, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R25, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }
                {
                    HObject ExpTmpOutVar_0;
                    HOperatorSet.ConcatObj(ho_Rects, ho_R26, out ExpTmpOutVar_0);
                    ho_Rects.Dispose();
                    ho_Rects = ExpTmpOutVar_0;
                }

                HOperatorSet.SetColor(hv_WinHandle, "gray");
                HOperatorSet.SetLineWidth(hv_WinHandle, 1);
                HOperatorSet.DispObj(ho_Rects, hv_WinHandle);
                ho_R0.Dispose();
                ho_R1.Dispose();
                ho_R2.Dispose();
                ho_R3.Dispose();
                ho_R4.Dispose();
                ho_R5.Dispose();
                ho_R6.Dispose();
                ho_R7.Dispose();
                ho_R8.Dispose();
                ho_R9.Dispose();
                ho_R10.Dispose();
                ho_R11.Dispose();
                ho_R12.Dispose();
                ho_R13.Dispose();
                ho_R14.Dispose();
                ho_R15.Dispose();
                ho_R16.Dispose();
                ho_R17.Dispose();
                ho_R18.Dispose();
                ho_R19.Dispose();
                ho_R20.Dispose();
                ho_R21.Dispose();
                ho_R22.Dispose();
                ho_R23.Dispose();
                ho_R24.Dispose();
                ho_R25.Dispose();
                ho_R26.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_R0.Dispose();
                ho_R1.Dispose();
                ho_R2.Dispose();
                ho_R3.Dispose();
                ho_R4.Dispose();
                ho_R5.Dispose();
                ho_R6.Dispose();
                ho_R7.Dispose();
                ho_R8.Dispose();
                ho_R9.Dispose();
                ho_R10.Dispose();
                ho_R11.Dispose();
                ho_R12.Dispose();
                ho_R13.Dispose();
                ho_R14.Dispose();
                ho_R15.Dispose();
                ho_R16.Dispose();
                ho_R17.Dispose();
                ho_R18.Dispose();
                ho_R19.Dispose();
                ho_R20.Dispose();
                ho_R21.Dispose();
                ho_R22.Dispose();
                ho_R23.Dispose();
                ho_R24.Dispose();
                ho_R25.Dispose();
                ho_R26.Dispose();

                throw HDevExpDefaultException;
            }
        }

        public void ShowRect(HObject ho_Image, HObject ho_Rects, HTuple hv_WinHandle,
            HTuple hv_RobotPos, HTuple hv_Result)
        {




            // Local iconic variables 

            HObject ho_Rectangle = null, ho_ImageReduced = null;
            HObject ho_R = null, ho_G = null, ho_B = null, ho_Region = null;
            HObject ho_RegionDilation = null;

            // Local control variables 

            HTuple hv_Number = null, hv_Clr = new HTuple();
            // Initialize local and output iconic variables 
            HOperatorSet.GenEmptyObj(out ho_Rectangle);
            HOperatorSet.GenEmptyObj(out ho_ImageReduced);
            HOperatorSet.GenEmptyObj(out ho_R);
            HOperatorSet.GenEmptyObj(out ho_G);
            HOperatorSet.GenEmptyObj(out ho_B);
            HOperatorSet.GenEmptyObj(out ho_Region);
            HOperatorSet.GenEmptyObj(out ho_RegionDilation);
            try
            {
                HOperatorSet.CountObj(ho_Rects, out hv_Number);
                if ((int)(new HTuple(hv_Number.TupleGreaterEqual(18))) != 0)
                {
                    ho_Rectangle.Dispose();
                    HOperatorSet.SelectObj(ho_Rects, out ho_Rectangle, hv_RobotPos + 1);
                    ho_ImageReduced.Dispose();
                    HOperatorSet.ReduceDomain(ho_Image, ho_Rectangle, out ho_ImageReduced);
                    ho_R.Dispose(); ho_G.Dispose(); ho_B.Dispose();
                    HOperatorSet.Decompose3(ho_ImageReduced, out ho_R, out ho_G, out ho_B);
                    ho_Region.Dispose();
                    HOperatorSet.Threshold(ho_G, out ho_Region, 0, 128);
                    ho_RegionDilation.Dispose();
                    HOperatorSet.DilationCircle(ho_Region, out ho_RegionDilation, 1.5);
                    hv_Clr = "green";
                    if ((int)(new HTuple(hv_Result.TupleEqual(0))) != 0)
                    {
                        hv_Clr = "red";
                    }
                    HOperatorSet.SetColor(hv_WinHandle, hv_Clr);
                    HOperatorSet.SetDraw(hv_WinHandle, "fill");
                    HOperatorSet.DispObj(ho_RegionDilation, hv_WinHandle);
                    HOperatorSet.SetDraw(hv_WinHandle, "margin");
                    HOperatorSet.SetLineWidth(hv_WinHandle, 2);
                    HOperatorSet.DispObj(ho_Rectangle, hv_WinHandle);
                    HOperatorSet.SetLineWidth(hv_WinHandle, 1);
                }
                ho_Rectangle.Dispose();
                ho_ImageReduced.Dispose();
                ho_R.Dispose();
                ho_G.Dispose();
                ho_B.Dispose();
                ho_Region.Dispose();
                ho_RegionDilation.Dispose();

                return;
            }
            catch (HalconException HDevExpDefaultException)
            {
                ho_Rectangle.Dispose();
                ho_ImageReduced.Dispose();
                ho_R.Dispose();
                ho_G.Dispose();
                ho_B.Dispose();
                ho_Region.Dispose();
                ho_RegionDilation.Dispose();

                throw HDevExpDefaultException;
            }
        }



        public void disp_message(HTuple hv_WindowHandle, HTuple hv_String, HTuple hv_CoordSystem,
            HTuple hv_Row, HTuple hv_Column, HTuple hv_Color, HTuple hv_Box)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_GenParamName = null, hv_GenParamValue = null;
            HTuple hv_Color_COPY_INP_TMP = hv_Color.Clone();
            HTuple hv_Column_COPY_INP_TMP = hv_Column.Clone();
            HTuple hv_CoordSystem_COPY_INP_TMP = hv_CoordSystem.Clone();
            HTuple hv_Row_COPY_INP_TMP = hv_Row.Clone();

            // Initialize local and output iconic variables 
            //This procedure displays text in a graphics window.
            //
            //Input parameters:
            //WindowHandle: The WindowHandle of the graphics window, where
            //   the message should be displayed
            //String: A tuple of strings containing the text message to be displayed
            //CoordSystem: If set to 'window', the text position is given
            //   with respect to the window coordinate system.
            //   If set to 'image', image coordinates are used.
            //   (This may be useful in zoomed images.)
            //Row: The row coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Column: The column coordinate of the desired text position
            //   A tuple of values is allowed to display text at different
            //   positions.
            //Color: defines the color of the text as string.
            //   If set to [], '' or 'auto' the currently set color is used.
            //   If a tuple of strings is passed, the colors are used cyclically...
            //   - if |Row| == |Column| == 1: for each new textline
            //   = else for each text position.
            //Box: If Box[0] is set to 'true', the text is written within an orange box.
            //     If set to' false', no box is displayed.
            //     If set to a color string (e.g. 'white', '#FF00CC', etc.),
            //       the text is written in a box of that color.
            //     An optional second value for Box (Box[1]) controls if a shadow is displayed:
            //       'true' -> display a shadow in a default color
            //       'false' -> display no shadow
            //       otherwise -> use given string as color string for the shadow color
            //
            //It is possible to display multiple text strings in a single call.
            //In this case, some restrictions apply:
            //- Multiple text positions can be defined by specifying a tuple
            //  with multiple Row and/or Column coordinates, i.e.:
            //  - |Row| == n, |Column| == n
            //  - |Row| == n, |Column| == 1
            //  - |Row| == 1, |Column| == n
            //- If |Row| == |Column| == 1,
            //  each element of String is display in a new textline.
            //- If multiple positions or specified, the number of Strings
            //  must match the number of positions, i.e.:
            //  - Either |String| == n (each string is displayed at the
            //                          corresponding position),
            //  - or     |String| == 1 (The string is displayed n times).
            //
            //
            //Convert the parameters for disp_text.
            if ((int)((new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(new HTuple())))) != 0)
            {

                return;
            }
            if ((int)(new HTuple(hv_Row_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Row_COPY_INP_TMP = 12;
            }
            if ((int)(new HTuple(hv_Column_COPY_INP_TMP.TupleEqual(-1))) != 0)
            {
                hv_Column_COPY_INP_TMP = 12;
            }
            //
            //Convert the parameter Box to generic parameters.
            hv_GenParamName = new HTuple();
            hv_GenParamValue = new HTuple();
            if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(0))) != 0)
            {
                if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleEqual("false"))) != 0)
                {
                    //Display no box
                    hv_GenParamName = hv_GenParamName.TupleConcat("box");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)(new HTuple(((hv_Box.TupleSelect(0))).TupleNotEqual("true"))) != 0)
                {
                    //Set a color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("box_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(0));
                }
            }
            if ((int)(new HTuple((new HTuple(hv_Box.TupleLength())).TupleGreater(1))) != 0)
            {
                if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleEqual("false"))) != 0)
                {
                    //Display no shadow.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat("false");
                }
                else if ((int)(new HTuple(((hv_Box.TupleSelect(1))).TupleNotEqual("true"))) != 0)
                {
                    //Set a shadow color other than the default.
                    hv_GenParamName = hv_GenParamName.TupleConcat("shadow_color");
                    hv_GenParamValue = hv_GenParamValue.TupleConcat(hv_Box.TupleSelect(1));
                }
            }
            //Restore default CoordSystem behavior.
            if ((int)(new HTuple(hv_CoordSystem_COPY_INP_TMP.TupleNotEqual("window"))) != 0)
            {
                hv_CoordSystem_COPY_INP_TMP = "image";
            }
            //
            if ((int)(new HTuple(hv_Color_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                //disp_text does not accept an empty string for Color.
                hv_Color_COPY_INP_TMP = new HTuple();
            }
            //
            HOperatorSet.DispText(hv_WindowHandle, hv_String, hv_CoordSystem_COPY_INP_TMP,
                hv_Row_COPY_INP_TMP, hv_Column_COPY_INP_TMP, hv_Color_COPY_INP_TMP, hv_GenParamName,
                hv_GenParamValue);

            return;
        }

        // Chapter: Graphics / Text
        // Short Description: Set font independent of OS 
        public void set_display_font(HTuple hv_WindowHandle, HTuple hv_Size, HTuple hv_Font,
            HTuple hv_Bold, HTuple hv_Slant)
        {



            // Local iconic variables 

            // Local control variables 

            HTuple hv_OS = null, hv_Fonts = new HTuple();
            HTuple hv_Style = null, hv_Exception = new HTuple(), hv_AvailableFonts = null;
            HTuple hv_Fdx = null, hv_Indices = new HTuple();
            HTuple hv_Font_COPY_INP_TMP = hv_Font.Clone();
            HTuple hv_Size_COPY_INP_TMP = hv_Size.Clone();

            // Initialize local and output iconic variables 
            //This procedure sets the text font of the current window with
            //the specified attributes.
            //
            //Input parameters:
            //WindowHandle: The graphics window for which the font will be set
            //Size: The font size. If Size=-1, the default of 16 is used.
            //Bold: If set to 'true', a bold font is used
            //Slant: If set to 'true', a slanted font is used
            //
            HOperatorSet.GetSystem("operating_system", out hv_OS);
            // dev_get_preferences(...); only in hdevelop
            // dev_set_preferences(...); only in hdevelop
            if ((int)((new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(new HTuple()))).TupleOr(
                new HTuple(hv_Size_COPY_INP_TMP.TupleEqual(-1)))) != 0)
            {
                hv_Size_COPY_INP_TMP = 16;
            }
            if ((int)(new HTuple(((hv_OS.TupleSubstr(0, 2))).TupleEqual("Win"))) != 0)
            {
                //Restore previous behaviour
                hv_Size_COPY_INP_TMP = ((1.13677 * hv_Size_COPY_INP_TMP)).TupleInt();
            }
            else
            {
                hv_Size_COPY_INP_TMP = hv_Size_COPY_INP_TMP.TupleInt();
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("Courier"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Courier";
                hv_Fonts[1] = "Courier 10 Pitch";
                hv_Fonts[2] = "Courier New";
                hv_Fonts[3] = "CourierNew";
                hv_Fonts[4] = "Liberation Mono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("mono"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Consolas";
                hv_Fonts[1] = "Menlo";
                hv_Fonts[2] = "Courier";
                hv_Fonts[3] = "Courier 10 Pitch";
                hv_Fonts[4] = "FreeMono";
                hv_Fonts[5] = "Liberation Mono";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("sans"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Luxi Sans";
                hv_Fonts[1] = "DejaVu Sans";
                hv_Fonts[2] = "FreeSans";
                hv_Fonts[3] = "Arial";
                hv_Fonts[4] = "Liberation Sans";
            }
            else if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual("serif"))) != 0)
            {
                hv_Fonts = new HTuple();
                hv_Fonts[0] = "Times New Roman";
                hv_Fonts[1] = "Luxi Serif";
                hv_Fonts[2] = "DejaVu Serif";
                hv_Fonts[3] = "FreeSerif";
                hv_Fonts[4] = "Utopia";
                hv_Fonts[5] = "Liberation Serif";
            }
            else
            {
                hv_Fonts = hv_Font_COPY_INP_TMP.Clone();
            }
            hv_Style = "";
            if ((int)(new HTuple(hv_Bold.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Bold";
            }
            else if ((int)(new HTuple(hv_Bold.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Bold";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Slant.TupleEqual("true"))) != 0)
            {
                hv_Style = hv_Style + "Italic";
            }
            else if ((int)(new HTuple(hv_Slant.TupleNotEqual("false"))) != 0)
            {
                hv_Exception = "Wrong value of control parameter Slant";
                throw new HalconException(hv_Exception);
            }
            if ((int)(new HTuple(hv_Style.TupleEqual(""))) != 0)
            {
                hv_Style = "Normal";
            }
            HOperatorSet.QueryFont(hv_WindowHandle, out hv_AvailableFonts);
            hv_Font_COPY_INP_TMP = "";
            for (hv_Fdx = 0; (int)hv_Fdx <= (int)((new HTuple(hv_Fonts.TupleLength())) - 1); hv_Fdx = (int)hv_Fdx + 1)
            {
                hv_Indices = hv_AvailableFonts.TupleFind(hv_Fonts.TupleSelect(hv_Fdx));
                if ((int)(new HTuple((new HTuple(hv_Indices.TupleLength())).TupleGreater(0))) != 0)
                {
                    if ((int)(new HTuple(((hv_Indices.TupleSelect(0))).TupleGreaterEqual(0))) != 0)
                    {
                        hv_Font_COPY_INP_TMP = hv_Fonts.TupleSelect(hv_Fdx);
                        break;
                    }
                }
            }
            if ((int)(new HTuple(hv_Font_COPY_INP_TMP.TupleEqual(""))) != 0)
            {
                throw new HalconException("Wrong value of control parameter Font");
            }
            hv_Font_COPY_INP_TMP = (((hv_Font_COPY_INP_TMP + "-") + hv_Style) + "-") + hv_Size_COPY_INP_TMP;
            HOperatorSet.SetFont(hv_WindowHandle, hv_Font_COPY_INP_TMP);
            // dev_set_preferences(...); only in hdevelop

            return;
        }

        public void DisplayText(string value)
        {
            if (InvokeRequired)
            {
                this.Invoke(new Action<string>(DisplayText), new object[] { value });
                return;
            }

        }

        private void StartTimerTh()
        {
            timerActive = true;
            Stopwatch timerWatch = new Stopwatch();
            DateTime dt = DateTime.Now;
            while (timerActive)
            {
                Thread.Sleep(100);
                int ms = (int)DateTime.Now.Subtract(dt).TotalMilliseconds;
                TimeSpan t = TimeSpan.FromMilliseconds(ms);
                string sec = t.TotalSeconds.ToString("0.0");
                DisplayText(sec);
            }
        }

        private void mniLoad_Click(object sender, EventArgs e)
        {
            /*
            string sName = contextMenuStrip1.SourceControl.Name;
            sName = sName.Remove(0, 5);
            int iPos = Convert.ToInt32(sName);
            LoadImage(iPos);
            */
        }
        #endregion

        #region OTHER

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Ksecilen = listBox1.SelectedItem.ToString();
            LoadWriteRegion();
            Tx_RName.Text = Ksecilen;
        }

        private void Lbx_Hobj_SelectedIndexChanged(object sender, EventArgs e)
        {
            string a = listBox1.SelectedItem.ToString();

            string b = Lbx_Hobj.SelectedItem.ToString();
            HOperatorSet.DispObj(CurImage, hWin1.HalconWindow);
            HOperatorSet.DispObj(CurImage2, hWin2.HalconWindow);
            string rDi = Myyol + a + "\\" + b;
            int satirno = Lbx_Hobj.SelectedIndex;
            Lbx_Labels.SelectedIndex = satirno;
            Lbx_Kamera.SelectedIndex = satirno;
            HOperatorSet.ReadRegion(out ho_RRecion, rDi);

            HOperatorSet.SetColor(hWin1.HalconWindow, "blue");
            HOperatorSet.DispObj(ho_RRecion, hWin1.HalconWindow);
            HOperatorSet.SetColor(hWin2.HalconWindow, "blue");
            HOperatorSet.DispObj(ho_RRecion, hWin2.HalconWindow);
        }



        #endregion

        private void button27_Click(object sender, EventArgs e)
        {
            // GrabImage(true);
            GrabImage2(true);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            tmrWenglor.Enabled = false;
            button1.BackColor = Color.Red;
        }

        private void button21_Click(object sender, EventArgs e)
        {
            tmrWenglor.Enabled = true;
            button1.BackColor = Color.Green;
        }

        private void Lbx_Kamera_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void button22_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            GrabNCheck(true);
        }

        private void button39_Click(object sender, EventArgs e)
        {
            HOperatorSet.CloseAllFramegrabbers();
            InitCameras();
            InitCameras2();

            //do control a reçete bilgisi gö

        }


        public void getDirectories(string _directoryPath, DataGridView _dataGridView)
        {
            try
            {
                string path = Application.StartupPath + "\\Settings\\Araba\\";
                if (_directoryPath != "")
                {
                    //Verilen yol'daki klasörleri alıp,diziye atıyoruz.
                    string[] directories = Directory.GetDirectories(_directoryPath);


                    //tüm klasör dönüyoruz.
                    foreach (string LoopFolder in directories)
                    {
                        //DirectoryInfo ile klasörün bilgilerini çekiyoruz.
                        DirectoryInfo dirInfoDirectory = new DirectoryInfo(LoopFolder);
                        //  _listbox.Items.Add("+" + dirInfoDirectory.Name);

                        //Klasör altında ki dosylarımızı listeliyoruz.
                        //   this.getFileInfo(LoopFolder, _listbox);

                        if (File.Exists(path + dirInfoDirectory.Name+".dat"))
                        {
                            string[] lines = File.ReadAllLines(path + dirInfoDirectory.Name+".dat");

                            try
                            {
                                int a = lines.Count();
                                if (lines.Length > 0)
                                {
                                    for (int i = 0; i < a; i++)
                                    {
                                        string[] vals;
                                        vals = lines[i].Split(';');
                                        _dataGridView.Rows.Add(dirInfoDirectory.Name, vals[0], vals[1], vals[2], vals[3]);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    }




                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "HATA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button44_Click(object sender, EventArgs e)
        {
            TabClear();
            this.tabPage1.Parent = this.tabControl1; //show
        }

        private void label2_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            string path = Application.StartupPath + "\\Settings\\Recete";
            getDirectories(path, dataGridView1);
        }

        private void button42_Click(object sender, EventArgs e)
        {
            string path = Application.StartupPath + "\\Settings\\Araba\\";
            using (StreamWriter sw = new StreamWriter(path + textBox14.Text+".dat"))
            {
                sw.WriteLine(textBox15.Text+";"+textBox19.Text + ";" + textBox17.Text + ";" + textBox20.Text);


                MessageBox.Show("Ayarlar Kayıt Edildi", "Ayarlar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                AddToLog("Save Config", "Configuration Saved Successfully", 9, Color.Blue);
            }
        }

        private void button43_Click(object sender, EventArgs e)
        {
            dataGridView1.Rows.Clear();
            string path = Application.StartupPath + "\\Settings\\Recete";
            getDirectories(path, dataGridView1);
        }
    }
}

#region REPORTS CLASS
public class FileMerge
{
    public FileMerge(string dest)
    {
        // hedef dosya adresini al
        Dest = dest;
    }

    string Dest;
    string[] Files;
    Encoding Enc = Encoding.UTF8;

    public void Load(params string[] files)
    {
        // dosya adreslerini al
        Files = files;
    }

    public bool Save()
    {
        StringBuilder data = new StringBuilder();

        foreach (string file in Files)
        {
            // böyle bir dosya yoksa bu adımı atla
            if (!File.Exists(file))
            {
                continue;
            }

            // dosya içeriğini oku
            data.Append(File.ReadAllText(file, Enc));
        }

        // hiç dosya okunmadıysa false döndür
        if (data.Length <= 0)
        {
            return false;
        }

        // alınan dosya içeriğini yeni dosyaya yaz
        File.WriteAllText(Dest, data.ToString(), Enc);
        return true;
    }

    public void SetEncoding(Encoding enc)
    {
        Enc = enc;
    }
}

class pdfAktar
{
    public static void pdfKaydet(DataGridView veriTablosu)
    {

        PdfPTable pdfTablosu = new PdfPTable(veriTablosu.ColumnCount);
        pdfTablosu.DefaultCell.Padding = 3;
        pdfTablosu.WidthPercentage = 100;
        pdfTablosu.HorizontalAlignment = Element.ALIGN_LEFT;
        pdfTablosu.DefaultCell.BorderWidth = 1;
        foreach (DataGridViewColumn sutun in veriTablosu.Columns)
        {
            PdfPCell pdfHucresi = new PdfPCell(new Phrase(sutun.HeaderText));
            pdfHucresi.BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY;
            pdfTablosu.AddCell(pdfHucresi);
        }
        foreach (DataGridViewRow satir in veriTablosu.Rows)
        {
            foreach (DataGridViewCell cell in satir.Cells)
            {
                if (cell.Value != null)
                {
                    pdfTablosu.AddCell(cell.Value.ToString());

                }

            }
        }

        SaveFileDialog dosyakaydet = new SaveFileDialog();
        string raporsaat = DateTime.Now.ToString("yyyyMMddHHmm");
        dosyakaydet.FileName = raporsaat + " Sistem Rapor Dosyası";
        dosyakaydet.InitialDirectory = Environment.SpecialFolder.Desktop.ToString();
        dosyakaydet.Filter = "PDF Dosyası|*.pdf";
        if (dosyakaydet.ShowDialog() == DialogResult.OK)
        {
            using (FileStream stream = new FileStream(dosyakaydet.FileName, FileMode.Create))
            {
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 10f, 0f);
                PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                pdfDoc.Add(pdfTablosu);
                pdfDoc.Close();
                stream.Close();
                MessageBox.Show("PDF dosyası başarıyla oluşturuldu!\n" + "Dosya Konumu: " + dosyakaydet.FileName, "İşlem Tamam");
            }
        }


    }
}

#endregion

//string[] filePaths = Directory.GetFiles(@"c:\Test\", "*.txt", SearchOption.TopDirectoryOnly);

//button69.BackColor = button69.BackColor == Color.Transparent? Color.LightGreen : Color.Transparent;


//plc.Write("DB13.DBX32.2", true);
// uint ass = (uint)plc.Read("DB301.DBD0");
// ushort reF = (ushort)plc.Read("DB36.DBW4");
//object re = plc.Read("DB36.DBW4");

//   var a = bytess[0];
//   double IstBosaltmaAdres = bytess[34];