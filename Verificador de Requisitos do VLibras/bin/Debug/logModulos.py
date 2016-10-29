# os ja vem instalado com o python, portanto nao ha risco de ImportError
import os
modulos =  {"matplotlib":       "matplotlib-1.5.3-cp27-cp27m-win32.whl",
			"numpy":            "numpy-1.11.2-cp27-none-win32.whl",
			"yaml":             "PyYAML-3.12-cp27-cp27m-win32.whl",
			"win32clipboard":   "pypiwin32-219-cp27-none-win32.whl",
			"winshell":         "winshell-0.6-cp27-none-any.whl",
			"future":           "future-0.15.2-py2-none-any.whl",
			"nltk":             "nltk-3.0.5-py2-none-any.whl",
			"nltk_tgrep":       "nltk_tgrep-1.0.6-py2-none-any.whl",
			"pyparsing":        "pyparsing-2.1.9-py2.py3-none-any.whl",
			"dateutil":         "python_dateutil-2.5.3-py2.py3-none-any.whl",
			"pytz":             "pytz-2016.7-py2.py3-none-any.whl",
			"six":              "six-1.10.0-py2.py3-none-any.whl"
			}

def checaModulo(mod):
	try:
		__import__('imp').find_module(mod)
		# f = open(log, "a")
		print("Modulo "+mod+" encontrado.") #f.write("Modulo "+mod+" encontrado.\n")
		# f.close()
	except ImportError:
		# f = open(log, "a")
		print("Modulo nao "+mod+" encontrado.") #f.write("Modulo nao "+mod+" encontrado.\n")
		# f.close()

def run():
	for mod, whl in modulos.iteritems():
		checaModulo(mod)


if __name__ == '__main__':
	try:
		log = "log do Vlibras.txt"
		os.chdir(os.getenv("USERPROFILE")+"\\Desktop\\")
		# f = open(log, "a")
		#f.write("Instalacao dos modulos do Python:\n\n")
		# f.close()
		run()
	except Exception as e:
		print("Ocorreu um erro ao verificar a instalacao dos modulos do Python 2.7:\n"+str(e))
	finally:
		quit()
