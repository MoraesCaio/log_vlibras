using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Verificador_de_Requisitos_do_VLibras{

    class Program{
        static int timeout = 2000;

    	static void createLog(string file){
    		if(!File.Exists(file)){
    			File.Create(file).Close();
    		}
    	}
    	static void cleanLog(string file){
			using (StreamWriter sw = File.CreateText(file)){
                sw.WriteLine("");
            }
        }
        static void writeLine(string file, string txt){
        	using (StreamWriter sw = File.AppendText(file)){
            	sw.WriteLine(txt);
        	}
        }
        static void write(string file, string txt){
        	using (StreamWriter sw = File.AppendText(file)){
            	sw.Write(txt);
        	}
        }
        static void logEnvVar(string file, string envVar, bool user){
        	string valueEnvVar;
        	if(user){
        		valueEnvVar = Environment.GetEnvironmentVariable(envVar,EnvironmentVariableTarget.User);
        		if(valueEnvVar == null){
        			writeLine(file, "Valor de "+envVar+" é nulo.\n");
        		}else{
        			writeLine(file, "Valor de "+envVar+" é :\n" + valueEnvVar + "\n");
        		}
        	}else{
        		valueEnvVar = Environment.GetEnvironmentVariable(envVar,EnvironmentVariableTarget.Machine);
        		if(valueEnvVar == null){
        			writeLine(file, "Valor de "+envVar+" é nulo.\n");
        		}else{
        			writeLine(file, "Valor de "+envVar+" é :\n" + valueEnvVar + "\n");
        		}
        	}
        }

        //[0]='32|'64';[1]='X.X.xx';[2]='X.X';[3]='xx'
        //para ser usada em conjunto com GetRegistroPythonPath[1 ou 2]()
        static string[] detalhesVerPython(string python){
            if(python == "" || python == null){
                string[] vazio = new string[4];
                return vazio;
            }else{
                ProcessStartInfo PSI1 = new ProcessStartInfo();
                PSI1.FileName = python;
                PSI1.Arguments = "--version";
                PSI1.UseShellExecute = false;
                PSI1.RedirectStandardError = true;
                PSI1.CreateNoWindow = true;

                Process process1 = Process.Start(PSI1);
                StreamReader reader = process1.StandardError;
                //completa é a versão completa
                string completa = reader.ReadToEnd();
                completa = completa.Remove(completa.Length - 2).Replace("Python ", "");
                string maior = completa.Remove(4);
                string menor = completa.Replace(maior, "");
                maior = maior.Remove(maior.Length - 1);
                process1.WaitForExit();
                //DETERMINAR SE EH X86 OU X64
                PSI1.Arguments = "arquiteturaPython.py";
                PSI1.RedirectStandardError = false;
                PSI1.RedirectStandardOutput = true;

                process1 = Process.Start(PSI1);
                reader = process1.StandardOutput;
                string arquitetura = reader.ReadToEnd();
                if(arquitetura.Contains("32") || arquitetura.Contains("64")){
                    arquitetura = arquitetura.Remove(2);
                }
                string[] resultado = new string[]{arquitetura, completa, maior, menor};
                process1.WaitForExit();
                return resultado;
            }
        }

        static string versaoPIP(string python){
            if(python == "" || python == null){
                return "";
            }else{
                string pip = python.Replace("python.exe", @"Scripts\pip.exe");
                ProcessStartInfo PSI = new ProcessStartInfo();
                PSI.FileName = pip;
                PSI.Arguments = "--version";
                PSI.UseShellExecute = false;
                PSI.RedirectStandardOutput = true;
                PSI.RedirectStandardError = true;
                PSI.CreateNoWindow = true;

                Process PS = Process.Start(PSI);
                StreamReader readerError = PS.StandardError;
                StreamReader readerOutput = PS.StandardOutput;
                string error = readerError.ReadToEnd();
                string output = readerOutput.ReadToEnd();
                string resultado = "Error: \n" + error + "\nOutput:\n" + output;
                PS.WaitForExit();
                return resultado;
            }
        }

        static string executarScriptPython(string python, string modulo){
            ProcessStartInfo procInfo = new ProcessStartInfo();
            procInfo.FileName = python;
            procInfo.Arguments = modulo;//"logModulos.py";
            procInfo.UseShellExecute = false;
            procInfo.RedirectStandardOutput = true;
            procInfo.CreateNoWindow = true;
            procInfo.WindowStyle = ProcessWindowStyle.Hidden;

            Process process = Process.Start(procInfo);
            StreamReader reader = process.StandardOutput;
            string resultado = reader.ReadToEnd();
            //Como escrever linha a linha
            /*string resultado = "";
            while(!reader.EndOfStream){
                //resultado = resultado + reader.ReadLine() + "\n";
                resultado = reader.ReadLine() + "\n";
                Console.Write(resultado);
                write(log, resultado); //tem que adicionar o parametro log
            }*/
            process.WaitForExit();
            return resultado;
        }

        static bool IsInstalled(RegistryKey[] regs){
    		foreach(RegistryKey reg in regs){
    			if(reg != null){
    				return true;
    			}
    		}
    		return false;
    	}

    	static string GetRegistroPythonPath(){
    		var lm64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            var lm32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        	var cu64 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry64);
            var cu32 = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Registry32);

    		RegistryKey pythonReg = null;
        	//LOCAL MACHINE x64(semPy)->nenhum;
            if(lm64.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = lm64.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath");
            }
            if(lm32.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = lm32.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath");
            }
            if(lm32.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = lm32.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath");
            }
            if(lm64.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = lm64.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath");
            }
            //CURRENT USER
            if(cu64.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = cu64.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath");
            }
            if(cu32.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = cu32.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath");
            }
            if(cu32.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = cu32.OpenSubKey(@"SOFTWARE\Python\PythonCore\2.7\InstallPath");
            }
            if(cu64.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath") != null){
                pythonReg = cu64.OpenSubKey(@"SOFTWARE\Wow6432Node\Python\PythonCore\2.7\InstallPath");
            }
        	if(pythonReg != null){
        		//CONIFGURAÇÃO DA VARIÁVEL PYTHONPATH
        		string python = pythonReg.GetValue(null).ToString();
        		//python = python.Remove(python.Length - 1).ToString();
	        	return "\""+python+@"python.exe"+"\"";
        	}else{
        		return "";
        	}
    	}

    	static string GetPathVLibras(){
    		string valueEnvVar = Environment.GetEnvironmentVariable("PATH_VLIBRAS",EnvironmentVariableTarget.User);
    		if(valueEnvVar == null){
    			return "";
    		}else{
    			return valueEnvVar+@"\";
    		}
    	}

    	private static bool RemoteFileExists(string url){
            bool result = false;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.Timeout = timeout;
            HttpWebResponse response = null;
            try{
                response = (HttpWebResponse)request.GetResponse();
                result = (response.StatusCode == HttpStatusCode.OK);
            }catch(Exception e){
                //Console.WriteLine("Arquivo não existe no servidor: {0}",e);
            }finally{
                if(response != null){
                    response.Close();
                }
            }
            return result;
        }

        private static bool RemoteIsNewerThanLocal(string url, string path){
            bool result = false;//resultado padrão
            //informações do arquivo
            FileInfo sourceFile = new FileInfo(path);
            //solicitação do arquivo remoto
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "HEAD";
            request.Timeout = timeout;
            HttpWebResponse response = null;
            try{
                response = (HttpWebResponse)request.GetResponse();
                result = (response.LastModified > sourceFile.LastWriteTime);
            }catch(Exception e){
                //Console.WriteLine("Erro: {0}",e);
            }finally{
                if(response != null){
                    response.Close();
                }
            }
            return result;
        }

        static void Main(string[] args){
            //ALGUMAS VARIÁVEIS
        	string url = @"http://atualizacao.vlibras.lavid.ufpb.br/windows/";
        	string dataFormatada = DateTime.Now.ToString("H:mm dd/MM/yyyy");
        	string data = DateTime.Now.ToString("Hmm-ddMMyyyy");
        	string desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        	string log = desktop + @"\log do Vlibras.txt";


            //NÃO ESTÁ USANDO ESSA PARTE
        	/*
            //REGISTRO DA INSTALAÇÃO DO PYTHON UTILIZADA NA VLIBRAS
			RegistryKey[] regPython = new RegistryKey[4];
            regPython[0] = lm64.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{9DA28CE5-0AA5-429E-86D8-686ED898C665}");
            regPython[1] = lm32.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{9DA28CE5-0AA5-429E-86D8-686ED898C665}");
            regPython[2] = lm32.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{9DA28CE5-0AA5-429E-86D8-686ED898C665}");
            regPython[3] = lm64.OpenSubKey(@"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\{9DA28CE5-0AA5-429E-86D8-686ED898C665}");*/


        	//CRIAÇÃO DO ARQUIVO DE LOG
        	createLog(log);
        	cleanLog(log);
        	writeLine(log, "Data: " + dataFormatada + "");


        	//ATALHOS
        	write(log, "Criação do atalho VLibras.lnk:");
        	if(!File.Exists(desktop+@"\VLibras.lnk")){
        		writeLine(log, " atalho NÃO existe!\n");
    		}else{
    			writeLine(log, " atalho criado.\n");
    		}
    		write(log, "Criação do atalho Atualizador VLibras:");
        	if(!File.Exists(desktop+@"\Atualizador VLibras.appref-ms")){
        		writeLine(log, " atalho NÃO existe!\n");
    		}else{
    			writeLine(log, " atalho criado.\n");
    		}


        	//VARIÁVEIS DE AMBIENTE
        	//USUÁRIO
        	string[] userEnvVar = new string[5];
        	userEnvVar[0] = "PATH_VLIBRAS";
        	userEnvVar[1] = "AELIUS_DATA";
        	userEnvVar[2] = "HUNPOS_TAGGER";
        	userEnvVar[3] = "NLTK_DATA";
        	userEnvVar[4] = "TRANSLATE_DATA";
        	try{
	        	foreach(string envVar in userEnvVar){
	        		logEnvVar(log, envVar, true);
	        	}
        	}catch(Exception e){
        		writeLine(log, "ERRO ao recuperar os valores das variáveis de ambiente de usuário:" + e);
        	}
        	//MÁQUINA
        	string[] machineEnvVar = new string[1];
        	machineEnvVar[0] = "PYTHONPATH";
        	try{
	        	foreach(string envVar in machineEnvVar){
	        		logEnvVar(log, envVar, false);
	        	}
        	}catch(Exception e){
        		writeLine(log, "ERRO ao recuperar os valores das variáveis de ambiente de máquina:" + e);
        	}

            write(log, "INFORMAÇÕES ACERCA DE ARQUITETURA E VERSÃO\n");
            //VERSÃO DO WINDOWS
            var SO = (from x in new ManagementObjectSearcher("SELECT Caption FROM Win32_OperatingSystem").Get().Cast<ManagementObject>()
                      select x.GetPropertyValue("Caption")).FirstOrDefault();
            if(SO != null){
                var win = SO.ToString();
                write(log, "Versão do windows: " + win.Remove(win.Length - 1)+";\n");
            }else{
                write(log, "Versão do windows: Não foi possível recuperar o nome do SO;\n");
            }


            //ARQUITETURA DA MÁQUINA DO USUÁRIO
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT UserName FROM Win32_ComputerSystem");
            ManagementObjectCollection collection = searcher.Get();
            string username = (string)collection.Cast<ManagementBaseObject>().First()["UserName"];
            write(log, "Nome do usuário: "+username+";\n");
            /*username = Environment.UserName;
            write(log, "Nome do usuário2: "+username+";\n");*/
            bool is64bit = !string.IsNullOrEmpty(
            Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"));
            if(is64bit){
                write(log, "Arquitetura dessa máquina: x64;\n");
            }else{
                write(log, "Arquitetura dessa máquina: x86;\n");
            }


            //INSTALAÇÃO PYTHON
            string python = GetRegistroPythonPath();
            if(python != ""){
                string[] detalhes = detalhesVerPython(python);
                string resultado;
                write(log, "Arquitetura do python: "+detalhes[0]);
                if(!detalhes[0].Contains("erro")){write(log, " bits;\n");}
                write(log, "Versão completa do python instalado: "+detalhes[1]+";\n");
                write(log, "Versão principal: "+detalhes[2]+";\n");
                write(log, "Versão de build: " + detalhes[3]+";\n");

                write(log, "PIP:\n"+versaoPIP(python)+";\n");

            	//MÓDULOS DO PYTHON
                write(log, "\nINSTALACAO DOS MODULOS DO PYTHON:\n");
	        	resultado = executarScriptPython(python, "logModulos.py");
                write(log, resultado);
                write(log, "\nTESTE DO PORTGLOSA:\n");
                resultado = executarScriptPython(python, "testeGlosa.py");
                write(log, resultado+"\n");
        	}else{
                write(log, "O Python 2.7 NÃO SE ENCONTRA INSTALADO!\n");
            }


            //ATALHOS
            string vlibras = GetPathVLibras();
            write(log, "\nVerificação do CorePlugin.dll:");
            if(!File.Exists(vlibras+@"Player\vlibrasPlayer_Data\Plugins\CorePlugin.dll")){
                writeLine(log, " plugin NÃO existe!\n");
            }else{
                writeLine(log, " plugin criado.\n");
            }


            //EXTRAÇÃO DE VLIBRAS.ZIP
            writeLine(log,"ANÁLISE DOS ZIPS DA BUILD:");
            if(vlibras != ""){
                //VERIFICAÇÃO PARA VER SE BAIXOU E EXTRAIU O VLIBRAS.zip
                if(File.Exists(vlibras+@"VLIBRAS.zip")){
                    writeLine(log, "VLIBRAS.zip foi baixado.");
                }else{
                    writeLine(log, "VLIBRAS.zip NÃO foi baixado!");
                }
                if(File.Exists(vlibras+@"extraido")){
                    writeLine(log, "VLIBRAS.zip foi extraído.");
                }else{
                    writeLine(log, "VLIBRAS.zip NÃO foi extraído!");
                }
                //VERIFICAÇÃO PARA VER SE BAIXOU E EXTRAIU O requisitos.zip
                if(File.Exists(vlibras+@"requisitos.zip")){
                    writeLine(log, "requisitos.zip foi baixado.");
                }else{
                    writeLine(log, "requisitos.zip NÃO foi baixado!");
                }
                if(File.Exists(vlibras+@"extraidoReq")){
                    writeLine(log, "requisitos.zip foi extraído.");
                }else{
                    writeLine(log, "requisitos.zip NÃO foi extraído!");
                }
                //VERIFICAÇÃO DOS PACOTES DE BUNDLES
                write(log,"\nANÁLISE DOS PACOTES DE SINAIS:\n");
                int i = 1;
                string remote;
                string local;
                string ext = ".zip";
                while(true){
                    remote = url+i.ToString()+ext;
                    local = vlibras+@"..\"+i.ToString()+ext;
                    if(!RemoteFileExists(remote)){
                        i--;
                        break;
                    }else{
                        if(!File.Exists(local)){
                            write(log, "O pacote "+i+" não foi baixado.\n");
                        }else{
                            if(RemoteIsNewerThanLocal(remote, local)){
                                write(log, "O pacote "+i+" está desatualizado.\n");
                            }else{
                                write(log, "O pacote "+i+" foi baixado.\n");
                            }
                        }
                    }
                    i++;
                }
                write(log, "Total de "+i+" pacotes verificados no servidor.\n");
                for(int j = 1; j <= i; j++){
                    if(File.Exists(vlibras+@"..\Bundles\"+j)){
                        write(log, "Extração do pacote "+j+": OK.\n");
                    }else{
                        write(log, "Extração do pacote "+j+": NÃO FOI EXTRAÍDO!\n");
                    }
                }
            }
            write(log, "\nFim do Log.");
        }
    }
}
