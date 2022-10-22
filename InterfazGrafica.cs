using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

#region CONSIGNA
/*
 * 
 *  Utilizando hilos concurrentes, implementar un sistema donde se cuenta con tres hilos “Aplicación
 *  x” (con x=1 a 3), cada una de las cuales envía trabajos a imprimir a la cola de impresión de
 *  un hilo “Impresora”.
 * 
 * 
 *  Cada Aplicación demora un tiempo aleatorio de entre 0,5 y 4.5 segundos en generar un nuevo
 *  documento para imprimir, y repite este proceso 100 veces. Imprime “Aplicación x imprimió su
 *  trabajo n, quedando en la posición z de la cola” cuando envía a la cola tal trabajo.
 * 
 * 
 *  La Impresora demora un tiempo aleatorio de entre 0,5 y 1,0 segundos en imprimir un trabajo, y
 *  dispone de una cola que sólo puede contener 5 trabajos simultáneos. Imprime “Impresora imprimiendo
 *  trabajo i (i-ésimo trabajo de impresión)” cada vez que inicia la impresión de un trabajo
 *  (cuando lo toma de la cola), siendo i el contador de trabajos que la impresora procesa.
 * 
 * 
 *  Aplicar sincronización para evitar que se pierdan trabajos de impresión o que la impresora produzca
 *  un error al tratar de tomar un trabajo de una cola vacía.
 * 
 * 
 *  ¡Lo ideal sería tener una interfaz gráfica que (en vez de mostrar los mensajes básicos indicados
 *  más arriba) muestre los contadores de trabajos impresos por cada aplicación, el contador de trabajos
 *  impresos por la impresora, el estado de la cola, y si se produjo un bloqueo en alguna aplicación
 *  o en la impresora! (¡¡¡Es más, la tarea de mantener esta interfaz –o parte de ella– se la
 *  podría encargar a otro hilo!!! –aunque no es la única manera de implementarlo–)
 * 
 */
#endregion

namespace ProductorConsumidor
{
    public partial class InterfazGrafica : Form
    {
        public InterfazGrafica()
        {
            InitializeComponent();
            //Con esta sentencia indico que primero se debe generar la interfaz grafica.
            if (!IsHandleCreated)
                CreateControl();

            //Instancio los semaforos
            empty = new Semaphore(initialCount: 5, maximumCount: 5);
            full = new Semaphore(initialCount: 0, maximumCount: 5);
            mutex = new Semaphore(initialCount: 1, maximumCount: 1);

            //instancio el buffer o cola de impresion
            queue = new ColaDeImpresion();

            //Instancio los hilos cada uno con su correspondiente metodo
            Impresora = new Thread(start: Imprimir);
            App_1 = new Thread(start: GenerarDocumento);
            App_2 = new Thread(start: GenerarDocumento);
            App_3 = new Thread(start: GenerarDocumento);

            //Asigno un nombre a cada hilo
            Impresora.Name = "Impresora";
            App_1.Name = "1";
            App_2.Name = "2";
            App_3.Name = "3";

            //Con esta sentencia indico que si el form principal se cierra, los hilos deben morir :c
            Impresora.IsBackground = true;
            App_1.IsBackground = true;
            App_2.IsBackground = true;
            App_3.IsBackground = true;
        }

        //VARIABLES
        Semaphore empty;
        Semaphore full;
        Semaphore mutex;
        ColaDeImpresion queue;
        Thread App_1;
        Thread App_2;
        Thread App_3;
        Thread Impresora;
        bool ejecutar = true;
        int trabajosImpresos = 0;

        //MÉTODO CONSUMIR
        private void Imprimir()
        {
            //SECCIÓN CRITICA
            while (ejecutar)
            {
                full.WaitOne();         //wait(full)
                mutex.WaitOne();        //wait(mutex)

                //Actualizo la interfaz grafica indicando el hilo en ejecución
                HiloEnEjecucion(nombre: Thread.CurrentThread.Name);

                //Obtengo un numero aletorio de tipo double
                double numeroAleatorio = NumerosAleatorios.NumeroAleatorio(max: 1);

                //Simulo la espera de 0.5 a 1 seg (Thread.Sleep trabaja con milisegundos y con datos enteros por lo que realizamos un parseo)
                Thread.Sleep(millisecondsTimeout: (int)(numeroAleatorio * 1000));

                //Obtengo el mensaje de la cola
                string mensaje = queue.Dequeue();

                //Incremento el indice correspondiente a la cantidad de trabajos impresos
                trabajosImpresos++;

                //Actualizo la interfaz grafica
                PonerMensaje(mensaje);

                //Actualizo el estado de la cola de impresión
                ActualizarColaImpresion(frente: queue.Frente);

                //FIN SECCIÓN CRITICA

                mutex.Release();        //Signal(mutex)
                empty.Release();        //Signal(empty)
            }
        }

        void PonerMensaje(string mensaje)
        {
            Action actualiza = () =>
            {
                txtSalida.Text += mensaje;
                lblTrabajosImpresos.Text = trabajosImpresos.ToString();
            };

            try
            {
                Invoke(actualiza);
            }
            catch (Exception)
            {
               // Al cerrar la aplicación la interfaz grafica podria estar aun ejecutando mensajes delegados por los demas hilos
               // Simplemente atrapo la error para evitar conflictos al cerrar la aplicación
            }
        }

        void TrabajosPorApp(int contador, string thread)
        {
            Action actualiza = () =>
            {
                switch (thread)
                {
                    case "1":
                        txtTrabajosImpresos_1.Text = (contador + 1).ToString();
                        break;
                    case "2":
                        txtTrabajosImpresos_2.Text = (contador + 1).ToString();
                        break;
                    case "3":
                        txtTrabajosImpresos_3.Text = (contador + 1).ToString();
                        break;
                }
            };

            try
            {
                Invoke(actualiza);
            }
            catch (Exception)
            {
                // Al cerrar la aplicación la interfaz grafica podria estar aun ejecutando mensajes delegados por los demas hilos
                // Simplemente atrapo la error para evitar conflictos al cerrar la aplicación
            }
        }

        void HiloEnEjecucion(string nombre)
        {
            Action actualiza = () =>
            {
                switch (nombre)
                {
                    case "1":
                        txtBloqueado_1.Text = "1";
                        txtBloqueado_2.Text = "0";
                        txtBloqueado_3.Text = "0";
                        lblEstadoCola.Text = "Bloqueada";
                        break;
                    case "2":
                        txtBloqueado_1.Text = "0";
                        txtBloqueado_2.Text = "1";
                        txtBloqueado_3.Text = "0";
                        lblEstadoCola.Text = "Bloqueada";
                        break;
                    case "3":
                        txtBloqueado_1.Text = "0";
                        txtBloqueado_2.Text = "0";
                        txtBloqueado_3.Text = "1";
                        lblEstadoCola.Text = "Bloqueada";
                        break;
                    case "Impresora":
                        txtBloqueado_1.Text = "0";
                        txtBloqueado_2.Text = "0";
                        txtBloqueado_3.Text = "0";
                        lblEstadoCola.Text = "Activa";
                        break;
                }
            };

            try
            {
                Invoke(actualiza);
            }
            catch (Exception)
            {
                // Al cerrar la aplicación la interfaz grafica podria estar aun ejecutando mensajes delegados por los demas hilos
                // Simplemente atrapo la error para evitar conflictos al cerrar la aplicación
            }
        }

        void ActualizarColaImpresion(int final = -1, int frente = -1)
        {
            Action actualiza = () =>
            {
                if (final != frente)
                {
                    if(final != -1)
                    {
                        switch (final)
                        {
                            case 0:
                                txtPos0.Text = "X";
                                break;
                            case 1:
                                txtPos1.Text = "X";
                                break;
                            case 2:
                                txtPos2.Text = "X";
                                break;
                            case 3:
                                txtPos3.Text = "X";
                                break;
                            case 4:
                                txtPos4.Text = "X";
                                break;
                        }
                    }

                    if(frente != -1)
                    {
                        switch (frente)
                        {
                            case 0:
                                txtPos4.Text = "";
                                break;
                            case 1:
                                txtPos0.Text = "";
                                break;
                            case 2:
                                txtPos1.Text = "";
                                break;
                            case 3:
                                txtPos2.Text = "";
                                break;
                            case 4:
                                txtPos3.Text = "";
                                break;
                        }
                    }
                }
                else
                {
                    switch (final)
                    {
                        case 0:
                            txtPos0.Text = "Lleno";

                            break;
                        case 1:
                            txtPos1.Text = "Lleno";
                            break;
                        case 2:
                            txtPos2.Text = "Lleno";
                            break;
                        case 3:
                            txtPos3.Text = "Lleno";
                            break;
                        case 4:
                            txtPos4.Text = "Lleno";
                            break;
                    }
                }
            };

            try
            {
                Invoke(actualiza);
            }
            catch (Exception)
            {
                // Al cerrar la aplicación la interfaz grafica podria estar aun ejecutando mensajes delegados por los demas hilos
                // Simplemente atrapo la error para evitar conflictos al cerrar la aplicación
            }
        }

        //MÉTODO PRODUCIR
        private void GenerarDocumento()
        {

            for (int i = 0; i < 100; i++)
            {
                //SECCIÓN CRITICA

                empty.WaitOne();        //wait(empty)
                mutex.WaitOne();        //wait(mutex)

                //Actualizo la interfaz grafica indicando el hilo en ejecución
                switch (Thread.CurrentThread.Name)
                {
                    case "1":
                        HiloEnEjecucion(nombre: Thread.CurrentThread.Name);
                        break;
                    case "2":
                        HiloEnEjecucion(nombre: Thread.CurrentThread.Name);
                        break;
                    case "3":
                        HiloEnEjecucion(nombre: Thread.CurrentThread.Name);
                        break;
                }

                //Obtengo un numero aletorio de tipo double
                double numeroAleatorio = NumerosAleatorios.NumeroAleatorio();

                //Simulo la espera de 0.5 a 4.5 seg (Thread.Sleep trabaja con milisegundos y con datos enteros por lo que realizamos un parseo)
                Thread.Sleep((int)(numeroAleatorio * 1000));

                //Genero el mensaje
                string mensaje = $"Aplicación {Thread.CurrentThread.Name} imprimió su trabajo {i}, quedando en la posición {(queue.Final + 1)} de la cola \n";

                //Envio el mensaje a la cola
                queue.Enqueue(dato: mensaje);

                //Actualizo la interfaz grafica indicando el hilo en ejecución
                switch (Thread.CurrentThread.Name)
                {
                    case "1":
                        TrabajosPorApp(contador: i, thread: Thread.CurrentThread.Name);
                        break;
                    case "2":
                        TrabajosPorApp(contador: i, thread: Thread.CurrentThread.Name);
                        break;
                    case "3":
                        TrabajosPorApp(contador: i, thread: Thread.CurrentThread.Name);
                        break;
                }

                //Actualizo el estado de la cola de impresión
                ActualizarColaImpresion(final: queue.Final);
                
                //FIN SECCIÓN CRITICA

                mutex.Release();        //Signal(mutex)
                full.Release();         //Signal(full)
            }

        }

        //EVENTO: se activa posterior a la instanciación de la interfaz grafica.
        private void InterfazGrafica_Activated(object sender, EventArgs e)
        {
            //Doy inicio a los hilos
            Impresora.Start();
            App_1.Start();
            App_2.Start();
            App_3.Start();
        }
    }
}
