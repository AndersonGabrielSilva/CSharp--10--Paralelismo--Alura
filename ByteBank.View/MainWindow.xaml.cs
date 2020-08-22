using ByteBank.Core.Model;
using ByteBank.Core.Repository;
using ByteBank.Core.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ByteBank.View
{
    public partial class MainWindow : Window
    {
        private readonly ContaClienteRepository r_Repositorio;
        private readonly ContaClienteService r_Servico;

        public MainWindow()
        {
            InitializeComponent();

            r_Repositorio = new ContaClienteRepository();
            r_Servico = new ContaClienteService();
        }

        private void BtnProcessar_Click(object sender, RoutedEventArgs e)
        {
            //Aula01_ExemploDeThread();
            //Aula02_ExemploDeTask();
            Aula03_UsandoAsyncAwait();



        }

        private async void Aula03_UsandoAsyncAwait()
        {
            BtnProcessar.IsEnabled = false;
            var contas = r_Repositorio.GetContaClientes();

            AtualizarView(new List<string>(), TimeSpan.Zero);
            var inicio = DateTime.Now;

            var resultado = await ConsolidarContas(contas);

            var fim = DateTime.Now;
            AtualizarView(resultado, fim - inicio);
            BtnProcessar.IsEnabled = true;

            #region Metodo de fazer sem o async await
            /*Metodo sem async await*/
            //ConsolidarContas(contas)
            //    .ContinueWith(task => //ContinueWith será execuldado somente quando todas as tarefas anteriores forem completadas,                                    
            //    {                     //O ContinueWith ecebe como parametro uma Task (Tarefa), que é justamente a que originou o metodo
            //        var resultado = task.Result;
            //        var fim = DateTime.Now;
            //        AtualizarView(resultado, fim - inicio);
            //    }, _taskSchedulerUI)
            //    .ContinueWith(task =>
            //    {
            //        BtnProcessar.IsEnabled = true;
            //    }, _taskSchedulerUI);
            #endregion
        }

        private async Task<string[]> ConsolidarContas(IEnumerable<ContaCliente> contas)
        {

            var _tasks = contas.Select(conta =>
                 Task.Factory.StartNew(() => r_Servico.ConsolidarMovimentacao(conta))
            );

            var resultado = await Task.WhenAll(_tasks);

            return resultado;

            #region Como realizar sem o Async Await
            //var _tasks = contas.Select(conta =>
            //{
            //    return Task.Factory.StartNew(() =>
            //    {
            //        var contaResultado = r_Servico.ConsolidarMovimentacao(conta);
            //        resultado.Add(contaResultado);
            //    });
            //}).ToArray();

            //return Task.WhenAll(_tasks)
            //    .ContinueWith(task =>
            //    {
            //        return resultado;
            //    });
            #endregion

        }

        private void Aula02_ExemploDeTask()
        {
            BtnProcessar.IsEnabled = false;

            //O TaskScheduler."FromCurrentSynchronizationContext() - obtem o contexto da thread que está aatuando no comento"
            var _taskSchedulerUI = TaskScheduler.FromCurrentSynchronizationContext();
            var contas = r_Repositorio.GetContaClientes();

            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);
            var inicio = DateTime.Now;

            //è classe responsavel para gerenciar as tarefas de acondo com oas threads disponoveis  
            //TaskScheduler

            var contasTarefas = contas.Select(conta =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var resultadoConta = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoConta);
                });
            }).ToArray();

            //Este metodo trava a execulção ate que todas as tarefas do array terminem sua execusão
            //Task.WaitAll(contasTarefas);

            //Com o 'WhenAll', é possivel retornar uma tarefa que será liberada somente quando todas as tarefas do array estiverem liberadas
            //com este metodo é possivel não travar a view 


            Task
                .WhenAll(contasTarefas)
                .ContinueWith(task => //ContinueWith será execuldado somente quando todas as tarefas anteriores forem completadas,                                    
                {                     //O ContinueWith ecebe como parametro uma Task (Tarefa), que é justamente a que originou o metodo
                    var fim = DateTime.Now;
                    AtualizarView(resultado, fim - inicio);
                }, _taskSchedulerUI)
                .ContinueWith(task =>
                {
                    BtnProcessar.IsEnabled = true;
                }, _taskSchedulerUI);
        }

        private void Aula01_ExemploDeThread()
        {
            var contas = r_Repositorio.GetContaClientes();

            var contasQuantidadePorThread = contas.Count() / 4;

            var contas_parte1 = contas.Take(contasQuantidadePorThread);
            var contas_parte2 = contas.Skip(contasQuantidadePorThread).Take(contasQuantidadePorThread);
            var contas_parte3 = contas.Skip(contasQuantidadePorThread * 2).Take(contasQuantidadePorThread);
            var contas_parte4 = contas.Skip(contasQuantidadePorThread * 3);

            var resultado = new List<string>();

            AtualizarView(new List<string>(), TimeSpan.Zero);

            var inicio = DateTime.Now;

            Thread thread_parte1 = new Thread(() =>
            {
                foreach (var conta in contas_parte1)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });
            Thread thread_parte2 = new Thread(() =>
            {
                foreach (var conta in contas_parte2)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });
            Thread thread_parte3 = new Thread(() =>
            {
                foreach (var conta in contas_parte3)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });
            Thread thread_parte4 = new Thread(() =>
            {
                foreach (var conta in contas_parte4)
                {
                    var resultadoProcessamento = r_Servico.ConsolidarMovimentacao(conta);
                    resultado.Add(resultadoProcessamento);
                }
            });

            thread_parte1.Start();
            thread_parte2.Start();
            thread_parte3.Start();
            thread_parte4.Start();

            while (thread_parte1.IsAlive || thread_parte2.IsAlive
                || thread_parte3.IsAlive || thread_parte4.IsAlive)
            {
                Thread.Sleep(250);
                //Não vou fazer nada
            }

            var fim = DateTime.Now;

            AtualizarView(resultado, fim - inicio);
        }

        private void AtualizarView(IEnumerable<string> result, TimeSpan elapsedTime)
        {
            var tempoDecorrido = $"{ elapsedTime.Seconds }.{ elapsedTime.Milliseconds} segundos!";
            var mensagem = $"Processamento de {result.Count()} clientes em {tempoDecorrido}";

            LstResultados.ItemsSource = result;
            TxtTempo.Text = mensagem;
        }
    }
}
