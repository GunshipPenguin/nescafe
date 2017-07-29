all:
	mcs -out:emulator.exe src/*.cs

clean:
	rm -f emulator.exe
