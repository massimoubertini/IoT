using System;
using System.Diagnostics;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace RaspberryDemo
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.InitLed();
            this.InitButton();
        }

        private GpioPin redPin;
        private GpioPin greenPin;
        private GpioPin bluePin;
        private GpioPin buttonPin;

        // timer per accendere e spegnere il LED
        private bool isOn;

        /* in un'app, l'evento clic si scatena quando si preme il pulsante;
           in quest'app il pulsante è H/W, in altre parole un modulo GPIO che
           può comunicare solo con valori binari, quindi si devono interpretare
           questi valori binari per capire quando il pulsante è premuto  */

        private void InitButton()
        {   // recupera il riferimento al controller GPIO
            var gpio = GpioController.GetDefault();
            // apre il pin del pulsante numero 27
            buttonPin = gpio.OpenPin(27);
            // si devono leggere dei valori, quindi è un pin d'input
            buttonPin.SetDriveMode(GpioPinDriveMode.Input);
            /* imposta l'intervallo di tempo per cui filtrare i cambi di stato
               si valorizza la proprietà DebounceTimeout, in pratica il tempo
               per cui i cambiamenti del valore del pin sono filtrati, quindi non
               si generano eventi ValueChanged; nell'esempio sono 50 ms
               così da evitare eventi spuri  causati da disturbi elettrici  */
            buttonPin.DebounceTimeout = TimeSpan.FromMilliseconds(50);
            /* si registra sull'evento ValueChanged che è generato quando
               lo stato del pin cambia da High a Low e viceversa */
            buttonPin.ValueChanged += (s, e) =>
            {   // lettura del pin
                var value = buttonPin.Read();
                /* stampa il valore nella finestra di Output
                   pulsante premuto = valore stampato: Low
                   pulsante rilasciato = valore stampato: High */
                Debug.WriteLine(value);
                if (value == GpioPinValue.High)
                {
                    // pulsante premuto
                    if (!isOn)
                    {
                        // scrive High sul pin per accendere il LED
                        redPin.Write(GpioPinValue.High);
                        isOn = true;
                    }
                    else
                    {
                        // scrive Low sul pin per spegnere il LED
                        redPin.Write(GpioPinValue.Low);
                        isOn = false;
                    }
                }
            };
        }

        private void InitLed()
        {   // recupera il riferimento al controller GPIO
            var gpio = GpioController.GetDefault();
            // apre i 3 pin
            redPin = gpio.OpenPin(18);
            greenPin = gpio.OpenPin(23);
            bluePin = gpio.OpenPin(24);
            // scrive Low sui 3 pin
            redPin.Write(GpioPinValue.Low);
            greenPin.Write(GpioPinValue.Low);
            bluePin.Write(GpioPinValue.Low);
            // sono pin di output
            redPin.SetDriveMode(GpioPinDriveMode.Output);
            greenPin.SetDriveMode(GpioPinDriveMode.Output);
            bluePin.SetDriveMode(GpioPinDriveMode.Output);
            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.5);
            // l’evento Tick del DispatcherTimer verifica il valore della variabile isOn
            timer.Tick += (s, e) =>
            {
                if (!isOn)
                {
                    // se è false si alza il pin RED
                    redPin.Write(GpioPinValue.High);
                    isOn = true;
                }
                else
                {
                    // se è true si abbassa il pin RED
                    redPin.Write(GpioPinValue.Low);
                    isOn = false;
                }
            };
            timer.Start();
        }
    }
}