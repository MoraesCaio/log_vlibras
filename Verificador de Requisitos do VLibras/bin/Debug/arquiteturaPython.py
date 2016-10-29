try:
	import platform
	t = platform.architecture()
	print(t[0])
except Exception as e:
	print("Ocorreu um erro ao verificar a arquitetura do Python 2.7:\n"+str(e))
finally:
	quit()