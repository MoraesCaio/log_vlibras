try:
	from PortGlosa import traduzir
	print(traduzir("o gato comeu o rato"))
except Exception as e:
	print("Ocorreu um erro ao testar o modulo \"traduzir\" do pacote \"PortGlosa\" do Python:\n"+str(e))
finally:
	quit()
