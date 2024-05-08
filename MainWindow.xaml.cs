using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows;

namespace _04_Moto_Proiettile_WPF
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private const double grav = 9.81;      // Accelerazione gravità in m/s^2
        private const double deltaT = 0.1;     // Intervallo di tempo in secondi
        private double _angolo;                // Angolo di lancio in gradi
        private double _forza;                 // Forza impressa in N
        private double _peso;                  // Peso del proiettile in kg
        private double _Vx;                    // Velocità iniziale asse x
        private double _Vy;                    // Velocità iniziale asse y
        private double _posx;                  // Posizione iniziale asse x
        private double _posy;                  // Posizione iniziale asse y

        private DispatcherTimer _timer;
        private Canvas _trajectoryCanvas; // Dichiarazione di _trajectoryCanvas

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            InitializeSim();
        }

        public double Angolo
        {
            get { return _angolo; }
            set
            {
                if (_angolo != value)
                {
                    _angolo = value;
                    OnPropertyChanged("Angolo");
                }
            }
        }

        public double Forza
        {
            get { return _forza; }
            set
            {
                if (_forza != value)
                {
                    _forza = value;
                    OnPropertyChanged("Forza");
                }
            }
        }

        public double Peso
        {
            get { return _peso; }
            set
            {
                if (_peso != value)
                {
                    _peso = value;
                    OnPropertyChanged("Peso");
                }
            }
        }

        private void InitializeSim()
        {
            // Inizializza la finestra
            this.SizeToContent = SizeToContent.Manual;

            // Crea un grid principale e aggiungilo alla finestra
            var grid = new Grid();
            this.Content = grid;

            // Aggiungi righe e colonne al grid per organizzare i controlli UI
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            // Aggiungi colonne al grid per organizzare i controlli UI
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            // Aggiungi controlli UI al grid
            var textAngolo = new TextBlock { Text = "Angolo di lancio (gradi):" };
            grid.Children.Add(textAngolo);
            Grid.SetRow(textAngolo, 0);
            Grid.SetColumn(textAngolo, 0);

            var txtAngolo = new TextBox { Name = "txtAngolo" };
            txtAngolo.SetBinding(TextBox.TextProperty, new Binding("Angolo") { Mode = BindingMode.TwoWay });
            grid.Children.Add(txtAngolo);
            Grid.SetRow(txtAngolo, 0);
            Grid.SetColumn(txtAngolo, 1);

            var textForza = new TextBlock { Text = "Forza impressa (N):" };
            grid.Children.Add(textForza);
            Grid.SetRow(textForza, 1);
            Grid.SetColumn(textForza, 0);

            var txtForza = new TextBox { Name = "txtForza" };
            txtForza.SetBinding(TextBox.TextProperty, new Binding("Forza") { Mode = BindingMode.TwoWay });
            grid.Children.Add(txtForza);
            Grid.SetRow(txtForza, 1);
            Grid.SetColumn(txtForza, 1);

            var textPeso = new TextBlock { Text = "Peso del proiettile (kg):" };
            grid.Children.Add(textPeso);
            Grid.SetRow(textPeso, 2);
            Grid.SetColumn(textPeso, 0);

            var txtPeso = new TextBox { Name = "txtPeso" };
            txtPeso.SetBinding(TextBox.TextProperty, new Binding("Peso") { Mode = BindingMode.TwoWay });
            grid.Children.Add(txtPeso);
            Grid.SetRow(txtPeso, 2);
            Grid.SetColumn(txtPeso, 1);

            var btnAvviaSimulazione = new Button { Content = "Avvia Simulazione" };
            btnAvviaSimulazione.Click += BtnAvviaSimulazione_Click;
            grid.Children.Add(btnAvviaSimulazione);
            Grid.SetRow(btnAvviaSimulazione, 3);
            Grid.SetColumn(btnAvviaSimulazione, 0);
            Grid.SetColumnSpan(btnAvviaSimulazione, 2);

            var btnClear = new Button { Content = "Clear" };
            btnClear.Click += BtnClear_Click;
            grid.Children.Add(btnClear);
            Grid.SetRow(btnClear, 4);
            Grid.SetColumn(btnClear, 0);
            Grid.SetColumnSpan(btnClear, 2);

            // Inizializza il canvas della traiettoria e aggiungilo al grid
            _trajectoryCanvas = new Canvas();
            grid.Children.Add(_trajectoryCanvas);
            Grid.SetRow(_trajectoryCanvas, 5);
            Grid.SetColumnSpan(_trajectoryCanvas, 2);
        }

        private void BtnAvviaSimulazione_Click(object sender, RoutedEventArgs e

)
        {
            if (Angolo <= 0 || Forza <= 0 || Peso <= 0)
            {
                MessageBox.Show("Si prega di inserire valori numerici validi (maggiore di zero) in tutte le caselle di testo.", "Errore di validazione", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Angolo: {Angolo}, Forza: {Forza}, Peso: {Peso}", "Dati validi", MessageBoxButton.OK, MessageBoxImage.Information);

            double angoloRad = Angolo * Math.PI / 180;
            _Vx = Forza * Math.Cos(angoloRad) / Peso;
            _Vy = Forza * Math.Sin(angoloRad) / Peso;

            // Resetta le coordinate d'origine della traiettoria
            _posx = 0;
            _posy = _trajectoryCanvas.ActualHeight;

            // Rimuovi la traiettoria precedente, se presente
            _trajectoryCanvas.Children.Clear();


            // Inizializza il timer per l'aggiornamento della simulazione
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(deltaT);
            _timer.Tick += Timer_Tick;
            _timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            double newPosX = _posx + _Vx * deltaT;
            double newPosY = _posy - (_Vy * deltaT - 0.5 * grav * Math.Pow(deltaT, 2));

            _Vy = _Vy - grav * deltaT; // Aggiorna Vy tenendo conto della gravità

            // Disegna la traiettoria sul canvas della traiettoria
            Line line = new Line();
            line.Stroke = Brushes.Red;
            line.StrokeThickness = 2;
            line.X1 = _posx;
            line.Y1 = _posy;
            line.X2 = newPosX;
            line.Y2 = newPosY;
            _trajectoryCanvas.Children.Add(line);

            _posx = newPosX;
            _posy = newPosY;

            if (_posy >= _trajectoryCanvas.ActualHeight)
            {
                _timer.Stop();
                MessageBox.Show("Proiettile atterrato.");
            }
        }

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            // Pulisci i dati di input
            Angolo = 0;
            Forza = 0;
            Peso = 0;

            // Rimuovi la traiettoria
            _trajectoryCanvas.Children.Clear();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}



