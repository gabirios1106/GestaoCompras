using System;
using System.Diagnostics;
using System.Threading;

class Program
{
    static void Main(string[] args)
    {
        while (true)
        {
            // Verifica se há algum processo docker com a porta 30497
            string checkCmd = "sudo ps -ef | grep docker | grep 30497 | wc -l";
            string output = RunBashCommand(checkCmd).Trim();

            if (int.TryParse(output, out int count))
            {
                int processosReais = count - 1; // Remove o processo do próprio grep

                if (processosReais <= 0)
                {
                    Console.WriteLine($"{DateTime.Now}: API parece estar parada. Reiniciando...");

                    string restartCmd = "cd /home/mesaderei && sudo docker compose down && sudo docker compose up -d";
                    RunBashCommand(restartCmd);
                }
                else
                {
                    Console.WriteLine($"{DateTime.Now}: API está rodando normalmente. ({processosReais} processo(s) ativo(s))");
                }
            }
            else
            {
                Console.WriteLine("❌ Erro ao converter a saída para número inteiro.");
            }

            Thread.Sleep(TimeSpan.FromMinutes(1));
        }
    }

    static string RunBashCommand(string command)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };

        process.Start();
        string result = process.StandardOutput.ReadToEnd();
        process.WaitForExit();
        return result;
    }
}
