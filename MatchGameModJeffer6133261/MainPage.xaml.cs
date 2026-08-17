using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MatchGameModJeffer6133261;

public partial class MainPage : ContentPage
{
    // lista de emojis del juego
    private List<string> emojis;
    // primera y segunda carta seleccionada
    private Label primeraCarta;
    private Label segundaCarta;
    // control del juego
    private bool esperando = false;
    private int paresEncontrados = 0;
    private int totalPares = 8;
    private bool juegoTerminado = false;
    private bool juegoIniciado = false;
    // cronometro
    private System.Timers.Timer timer;
    private int segundos = 0;
    // diccionario para guardar que emoji tiene cada carta
    private Dictionary<Label, string> emojisPorCarta;

    public MainPage()
    {
        InitializeComponent();
        IniciarJuego();
    }

    // reinicia todo el juego
    private void IniciarJuego()
    {
        // paramos el cronometro
        if (timer != null)
        {
            timer.Stop();
            timer.Dispose();
            timer = null;
        }

        // reiniciamos el tiempo
        segundos = 0;
        TiempoLabel.Text = "⏱️ Tiempo: 0 segundos";
        MensajeGanaste.IsVisible = false;

        // reiniciamos variables
        primeraCarta = null;
        segundaCarta = null;
        esperando = false;
        paresEncontrados = 0;
        juegoTerminado = false;
        juegoIniciado = false;
        emojisPorCarta = new Dictionary<Label, string>();

        // creamos los emojis (cada uno 2 veces)
        List<string> emojisTemp = new List<string>
        {
            "🐶", "🐶",
            "🐱", "🐱",
            "🐭", "🐭",
            "🐹", "🐹",
            "🐰", "🐰",
            "🦊", "🦊",
            "🐻", "🐻",
            "🐼", "🐼"
        };

        // barajamos los emojis
        Random random = new Random();
        for (int i = 0; i < emojisTemp.Count; i++)
        {
            int pos = random.Next(emojisTemp.Count);
            string temp = emojisTemp[i];
            emojisTemp[i] = emojisTemp[pos];
            emojisTemp[pos] = temp;
        }

        // ponemos los emojis en las cartas y los guardamos en el diccionario
        int indice = 0;
        foreach (Label label in Grid1.Children)
        {
            string emoji = emojisTemp[indice];
            emojisPorCarta[label] = emoji;  // guardamos el emoji de esta carta
            label.Text = "?";  // mostramos "?" boca abajo
            label.BackgroundColor = Color.FromArgb("#F0F0F0");
            label.IsEnabled = true;
            indice++;
        }
    }

    // mezcla solo los emojis que no se han encontrado
    private void ReagruparEmojisNoEncontrados()
    {
        // guardamos los emojis de las cartas no verdes
        List<string> emojisRestantes = new List<string>();
        List<Label> cartasRestantes = new List<Label>();

        foreach (Label label in Grid1.Children)
        {
            if (label.BackgroundColor != Color.FromArgb("#90EE90"))
            {
                // guardamos el emoji de esta carta
                emojisRestantes.Add(emojisPorCarta[label]);
                cartasRestantes.Add(label);
            }
        }

        // barajamos los emojis restantes
        Random random = new Random();
        for (int i = 0; i < emojisRestantes.Count; i++)
        {
            int pos = random.Next(emojisRestantes.Count);
            string temp = emojisRestantes[i];
            emojisRestantes[i] = emojisRestantes[pos];
            emojisRestantes[pos] = temp;
        }

        // los colocamos en las cartas no verdes
        for (int i = 0; i < cartasRestantes.Count; i++)
        {
            Label label = cartasRestantes[i];
            emojisPorCarta[label] = emojisRestantes[i];  // actualizamos el diccionario
            // solo mostramos el emoji si esta boca arriba, sino mostramos "?"
            if (label.BackgroundColor == Color.FromArgb("#FFFFFF"))
            {
                label.Text = emojisRestantes[i];
            }
            else
            {
                label.Text = "?";
            }
        }
    }

    // cuando tocamos una carta
    private void OnLabelTapped(object sender, EventArgs e)
    {
        if (juegoTerminado || esperando)
            return;

        Label carta = (Label)sender;

        // si ya esta descubierta o encontrada
        if (carta.Text != "?" || carta.BackgroundColor == Color.FromArgb("#90EE90"))
            return;

        // empezamos el cronometro en el primer toque
        if (!juegoIniciado)
        {
            juegoIniciado = true;
            IniciarCronometro();
        }

        // mostramos el emoji de la carta (lo sacamos del diccionario)
        carta.Text = emojisPorCarta[carta];
        carta.BackgroundColor = Color.FromArgb("#FFFFFF");

        // guardamos la primera carta
        if (primeraCarta == null)
        {
            primeraCarta = carta;
        }
        // guardamos la segunda carta
        else if (segundaCarta == null && carta != primeraCarta)
        {
            segundaCarta = carta;

            // comparamos los emojis (los sacamos del diccionario)
            string emoji1 = emojisPorCarta[primeraCarta];
            string emoji2 = emojisPorCarta[segundaCarta];

            if (emoji1 == emoji2)
            {
                // encontramos pareja
                primeraCarta.BackgroundColor = Color.FromArgb("#90EE90");
                segundaCarta.BackgroundColor = Color.FromArgb("#90EE90");
                primeraCarta.IsEnabled = false;
                segundaCarta.IsEnabled = false;
                paresEncontrados++;

                primeraCarta = null;
                segundaCarta = null;

                if (paresEncontrados == totalPares)
                {
                    TerminarJuego();
                }
            }
            else
            {
                // no son iguales, las giramos y mezclamos
                esperando = true;
                Device.StartTimer(TimeSpan.FromSeconds(1), () =>
                {
                    if (!juegoTerminado)
                    {
                        // las ponemos boca abajo
                        primeraCarta.Text = "?";
                        primeraCarta.BackgroundColor = Color.FromArgb("#F0F0F0");
                        segundaCarta.Text = "?";
                        segundaCarta.BackgroundColor = Color.FromArgb("#F0F0F0");

                        // MEZCLAMOS SOLO LOS EMOJIS QUE NO SE HAN ENCONTRADO
                        ReagruparEmojisNoEncontrados();
                    }

                    primeraCarta = null;
                    segundaCarta = null;
                    esperando = false;
                    return false;
                });
            }
        }
    }

    // inicia el cronometro
    private void IniciarCronometro()
    {
        timer = new System.Timers.Timer();
        timer.Interval = 1000;
        timer.Elapsed += (sender, e) =>
        {
            segundos++;
            Device.BeginInvokeOnMainThread(() =>
            {
                TiempoLabel.Text = $"⏱️ Tiempo: {segundos} segundos";
            });
        };
        timer.Start();
    }

    // cuando ganamos
    private void TerminarJuego()
    {
        juegoTerminado = true;
        if (timer != null)
        {
            timer.Stop();
        }
        MensajeGanaste.IsVisible = true;
        foreach (Label label in Grid1.Children)
        {
            label.IsEnabled = false;
        }
    }

    // boton reiniciar
    private void OnReiniciarClicked(object sender, EventArgs e)
    {
        IniciarJuego();
    }
}