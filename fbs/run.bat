flatc 	--gen-object-api --object-prefix "" --object-suffix "_FBS" --force-empty ^
	--cpp --cpp-ptr-type std::shared_ptr --gen-name-strings ^
	-o cpp ^
	--gen-compare ^
	enumPackets.fbs ^
	structs.fbs ^
	networkmessages.fbs 


flatc 	--gen-object-api --object-prefix "" --object-suffix "_FBS" --force-empty ^
	--csharp --gen-onefile ^
	-o csharp ^
	--gen-compare ^
	enumPackets.fbs ^
	structs.fbs ^
	networkmessages.fbs 
pause

