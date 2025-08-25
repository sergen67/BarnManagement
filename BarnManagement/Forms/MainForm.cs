using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BarnManagement.WinForms.Models;
using BarnManagement.WinForms.Models.Entities;

namespace BarnManagement.Forms
{
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm> _logger;
        private User _currentUser;
        private Timer _productTimer;
        private Animal _selectedAnimal;
        private int _produceTicks, _produceTargetTicks;

        public MainForm()
        {
            InitializeComponent();
            _logger = Program.Services.GetRequiredService<ILogger<MainForm>>();

            InitUiDefaults();     
            SetupTimer();
            

            this.Load += (s, e) => { EnsureTestProduct(); LoadGrids(); RefreshBalance(); RefreshAnimalCount();  };

            numUnitPrice.Minimum = 0; numUnitPrice.Maximum = 100000;
            numUnitPrice.DecimalPlaces = 2; numUnitPrice.Increment = 0.5M;

            dgvProducts.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvProducts.ReadOnly = true; dgvProducts.MultiSelect = false;

            lblStatus.Text = "Hazır";
        }


        public void Initialize(User user) { _currentUser = user; }
        private void SetupTimer()
        {
            _productTimer = new Timer() { Interval = 200 };
            _productTimer.Tick += (s, e) =>
            {
                _produceTicks++;
                progressBar1.Value = Math.Min(100, (int)(_produceTicks * 100.0 / _produceTargetTicks));
                if (_produceTicks >= _produceTargetTicks)
                {
                    _productTimer.Stop();
                    DoPersistProduction();
                }
            };
        }
        private void EnsureTestProduct()
        {
            using (var db = new BarnContext())
            {
                if (!db.Products.Any())
                {
                    var a = db.Animals.FirstOrDefault(x => x.IsAlive);
                    if (a == null)
                    {

                        db.Animals.Add(new Animal { Type = "Cow", Gender = "Female", AgeDays = 100, LifetimeDays = 1500, IsAlive = true });
                        db.SaveChanges();
                        a = db.Animals.First(x => x.IsAlive);
                    }
                    db.Products.Add(new Product { AnimalId = a.Id, ProductType = "Milk", Quantity = 5m });
                    db.SaveChanges();
                }
            }
        }
        private void InitUiDefaults()
        {
      
            if (cmbType.Items.Count == 0)
                cmbType.Items.AddRange(new object[] { "Cow", "Chicken", "Sheep" });

            if (cmbGender.Items.Count == 0)
                cmbGender.Items.AddRange(new object[] { "Female", "Male" });

            cmbType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGender.DropDownStyle = ComboBoxStyle.DropDownList;

            if (cmbType.SelectedIndex < 0) cmbType.SelectedIndex = 0;
            if (cmbGender.SelectedIndex < 0) cmbGender.SelectedIndex = 0;
        }

        private void LoadGrids()
        {
            try
            {
                using (var db = new BarnContext())
                {

                    dgvAnimals.AutoGenerateColumns = true;
                    dgvAnimals.DataSource = db.Animals
                                                .Where(a => a.IsAlive)
                                                .OrderByDescending(a => a.Id)
                                                .ToList();


                    var products = db.Products
                        .Where(p => !p.IsSold)
                        .Select(p => new { p.Id, p.AnimalId, p.ProductType, p.Quantity, p.ProducedAt })
                        .OrderByDescending(p => p.Id)
                        .ToList();

                    dgvProducts.AutoGenerateColumns = true;
                    dgvProducts.DataSource = null;
                    dgvProducts.DataSource = products;


                    var sold = db.Sales
                                .OrderByDescending(s => s.Id)
                                .Select( s => new  { s.Id,s.Quantity, s.SoldAt, s.UnitPrice })
                                .ToList();

                    dgvSoldProducts.AutoGenerateColumns = true;
                    dgvSoldProducts.DataSource = null;
                    dgvSoldProducts.DataSource = sold;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading grids");
                MessageBox.Show("Veriler yüklenirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshBalance()
        {
            try
            {
                using (var db = new BarnContext())
                {
                    var bal = db.Sales.Select(s => s.UnitPrice * s.Quantity).DefaultIfEmpty(0).Sum();
                    lblBalance.Text = $"Bakiye: {bal:C}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing balance");
            }
        }

        private void btnAddAnimal_Click(object sender, EventArgs e)
        {
            try
            {
                var type = cmbType.SelectedItem as string;
                var gender = cmbGender.SelectedItem as string;
                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(gender))
                { MessageBox.Show("Tür ve cinsiyet seçmelisin."); return; }

                using (var db = new BarnContext())
                {


                    db.Animals.Add(new Animal
                    {
                        Type = type,
                        Gender = gender,
                        AgeDays = (int)numAge.Value,
                        LifetimeDays = (int)numLifeTime.Value, // dikkat: Designer adıyla aynı olsun
                        IsAlive = true
                    });

                    db.SaveChanges();
                }

                LoadGrids();
                RefreshAnimalCount();

            }
            catch (Exception ex) { _logger.LogError(ex, "AddAnimal failed"); }
        }

        private void btnMarkDead_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvAnimals.CurrentRow != null && dgvAnimals.CurrentRow.DataBoundItem is Animal a)
                {
                    using (var db = new BarnContext())
                    {
                        var ent = db.Animals.Find(a.Id);
                        if (ent != null && ent.IsAlive)
                        {
                            ent.IsAlive = false;
                            var barn = db.Barns.First();
                            barn.CurrentAnimalCount = Math.Max(0, barn.CurrentAnimalCount - 1);
                            db.SaveChanges();
                        }
                    }
                    LoadGrids();
                    RefreshAnimalCount();

                }
                else { MessageBox.Show("Lütfen bir hayvan seçin."); }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MarkDead");
                MessageBox.Show("İşlem başarısız.");
            }
        }


        private void btnProduce_Click(object sender, EventArgs e)
        {
            try
            {
                if (!(dgvAnimals.CurrentRow?.DataBoundItem is Animal animal))
                { MessageBox.Show("Lütfen bir hayvan seçin."); return; }

                if (!animal.IsAlive || animal.AgeDays >= animal.LifetimeDays)   // ← FIX
                { MessageBox.Show("Bu hayvan üretimde değil."); return; }

                _selectedAnimal = animal;
                _produceTicks = 0;
                _produceTargetTicks = animal.Type == "Chicken" ? 10 :
                                      animal.Type == "Cow" ? 25 : 20;

                progressBar1.Value = 0;
                lblStatus.Text = "Üretim başladı...";
                _productTimer.Start();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Produce start failed");
                MessageBox.Show("Üretim başlatılırken hata.");
            }
        }

        private void DoPersistProduction()
        {
            try
            {
                using (var db = new BarnContext())
                {
                    var animal = db.Animals.Find(_selectedAnimal.Id);
                    if (animal == null || !animal.IsAlive || animal.AgeDays >= animal.LifetimeDays)
                    { lblStatus.Text = "Üretim iptal."; return; }

                    string p; decimal q;
                    switch (animal.Type)
                    {
                        case "Cow": p = "Milk"; q = 5.0m; break;
                        case "Chicken": p = "Egg"; q = 1.0m; break;
                        case "Sheep": p = "Wool"; q = 0.5m; break;
                        default: p = "Generic"; q = 1.0m; break;
                    }

                    db.Products.Add(new Product { AnimalId = animal.Id, ProductType = p, Quantity = q });
                    db.SaveChanges();
                }
                lblStatus.Text = "Üretim tamamlandı.";
                LoadGrids();
            }
            catch (Exception ex) { _logger.LogError(ex, "PersistProduction"); lblStatus.Text = "Üretim kaydı başarısız."; }
        }

        private void btnSell_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProducts.CurrentRow == null)
                { MessageBox.Show("Önce listeden ürün seç."); return; }

                var idObj = dgvProducts.CurrentRow.Cells["Id"].Value;   // 🔑 Id hücresi
                if (idObj == null) { MessageBox.Show("Geçersiz seçim."); return; }
                var prodId = Convert.ToInt32(idObj);

                var price = (decimal)numUnitPrice.Value;
                if (price <= 0) { MessageBox.Show("Birim fiyat > 0 olmalı."); return; }

                using (var db = new BarnContext())
                {
                    var prod = db.Products.Find(prodId);
                    if (prod == null || prod.IsSold) { MessageBox.Show("Ürün bulunamadı / satılmış."); return; }

                    db.Sales.Add(new Sale { ProductId = prod.Id, UnitPrice = price, Quantity = prod.Quantity });
                    prod.IsSold = true; // trigger varsa opsiyonel
                    db.SaveChanges();
                }

                LoadGrids();
                RefreshBalance();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Sell failed");
                MessageBox.Show("Satış başarısız.");
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadGrids();
            RefreshBalance();
        }

        private void RefreshAnimalCount()
        {
            try
            {
                using (var db = new BarnContext())
                {
                    var live = db.Animals.Count(a => a.IsAlive);
                    lblAnimalCount.Text = $"Animals: {live}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RefreshAnimalCount failed");
            }
        }
    }
}
