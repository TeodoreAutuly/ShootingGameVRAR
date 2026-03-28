#include "pch-c.h"


#include "codegen/il2cpp-codegen-metadata.h"





#if IL2CPP_MONO_DEBUGGER
static const Il2CppMethodExecutionContextInfo g_methodExecutionContextInfos[4] = 
{
	{ 38820, 0,  1 },
	{ 29553, 1,  2 },
	{ 45658, 2,  3 },
	{ 45658, 3,  3 },
};
#else
static const Il2CppMethodExecutionContextInfo g_methodExecutionContextInfos[1] = { { 0, 0, 0 } };
#endif
#if IL2CPP_MONO_DEBUGGER
static const char* g_methodExecutionContextInfoStrings[4] = 
{
	"networkManager",
	"bootstrapPlayer",
	"oldPosition",
	"newPosition",
};
#else
static const char* g_methodExecutionContextInfoStrings[1] = { NULL };
#endif
#if IL2CPP_MONO_DEBUGGER
static const Il2CppMethodExecutionContextInfoIndex g_methodExecutionContextInfoIndexes[12] = 
{
	{ 0, 0 },
	{ 0, 0 },
	{ 0, 2 },
	{ 0, 0 },
	{ 2, 2 },
	{ 0, 0 },
	{ 0, 0 },
	{ 0, 0 },
	{ 0, 0 },
	{ 0, 0 },
	{ 0, 0 },
	{ 0, 0 },
};
#else
static const Il2CppMethodExecutionContextInfoIndex g_methodExecutionContextInfoIndexes[1] = { { 0, 0} };
#endif
#if IL2CPP_MONO_DEBUGGER
IL2CPP_EXTERN_C Il2CppSequencePoint g_sequencePointsBootstrap[];
Il2CppSequencePoint g_sequencePointsBootstrap[119] = 
{
	{ 140381, 0, 0, 0, 0, 0, -1, kSequencePointKind_Normal, 0, 0 },
	{ 140381, 0, 0, 0, 0, 0, 16777215, kSequencePointKind_Normal, 0, 1 },
	{ 140381, 1, 44, 44, 9, 10, 0, kSequencePointKind_Normal, 0, 2 },
	{ 140381, 1, 45, 51, 13, 15, 1, kSequencePointKind_Normal, 0, 3 },
	{ 140381, 1, 45, 51, 13, 15, 27, kSequencePointKind_StepOut, 0, 4 },
	{ 140381, 1, 45, 51, 13, 15, 52, kSequencePointKind_StepOut, 0, 5 },
	{ 140381, 1, 52, 52, 9, 10, 90, kSequencePointKind_Normal, 0, 6 },
	{ 140383, 0, 0, 0, 0, 0, -1, kSequencePointKind_Normal, 0, 7 },
	{ 140383, 0, 0, 0, 0, 0, 16777215, kSequencePointKind_Normal, 0, 8 },
	{ 140383, 2, 12, 12, 9, 10, 0, kSequencePointKind_Normal, 0, 9 },
	{ 140383, 2, 13, 13, 13, 61, 1, kSequencePointKind_Normal, 0, 10 },
	{ 140383, 2, 13, 13, 13, 61, 21, kSequencePointKind_StepOut, 0, 11 },
	{ 140383, 2, 13, 13, 13, 61, 26, kSequencePointKind_StepOut, 0, 12 },
	{ 140383, 2, 15, 15, 13, 59, 32, kSequencePointKind_Normal, 0, 13 },
	{ 140383, 2, 15, 15, 13, 59, 32, kSequencePointKind_StepOut, 0, 14 },
	{ 140383, 2, 16, 16, 13, 70, 38, kSequencePointKind_Normal, 0, 15 },
	{ 140383, 2, 16, 16, 13, 70, 39, kSequencePointKind_StepOut, 0, 16 },
	{ 140383, 2, 16, 16, 13, 70, 47, kSequencePointKind_StepOut, 0, 17 },
	{ 140383, 2, 16, 16, 0, 0, 59, kSequencePointKind_Normal, 0, 18 },
	{ 140383, 2, 17, 17, 13, 14, 62, kSequencePointKind_Normal, 0, 19 },
	{ 140383, 2, 18, 18, 17, 46, 63, kSequencePointKind_Normal, 0, 20 },
	{ 140383, 2, 18, 18, 17, 46, 68, kSequencePointKind_StepOut, 0, 21 },
	{ 140383, 2, 18, 18, 17, 46, 73, kSequencePointKind_StepOut, 0, 22 },
	{ 140383, 2, 18, 18, 0, 0, 79, kSequencePointKind_Normal, 0, 23 },
	{ 140383, 2, 19, 19, 17, 18, 82, kSequencePointKind_Normal, 0, 24 },
	{ 140383, 2, 20, 20, 21, 48, 83, kSequencePointKind_Normal, 0, 25 },
	{ 140383, 2, 20, 20, 21, 48, 84, kSequencePointKind_StepOut, 0, 26 },
	{ 140383, 2, 21, 21, 17, 18, 90, kSequencePointKind_Normal, 0, 27 },
	{ 140383, 2, 23, 23, 17, 48, 91, kSequencePointKind_Normal, 0, 28 },
	{ 140383, 2, 23, 23, 17, 48, 96, kSequencePointKind_StepOut, 0, 29 },
	{ 140383, 2, 23, 23, 17, 48, 101, kSequencePointKind_StepOut, 0, 30 },
	{ 140383, 2, 23, 23, 0, 0, 107, kSequencePointKind_Normal, 0, 31 },
	{ 140383, 2, 24, 24, 17, 18, 110, kSequencePointKind_Normal, 0, 32 },
	{ 140383, 2, 25, 25, 21, 50, 111, kSequencePointKind_Normal, 0, 33 },
	{ 140383, 2, 25, 25, 21, 50, 112, kSequencePointKind_StepOut, 0, 34 },
	{ 140383, 2, 26, 26, 17, 18, 118, kSequencePointKind_Normal, 0, 35 },
	{ 140383, 2, 28, 28, 17, 48, 119, kSequencePointKind_Normal, 0, 36 },
	{ 140383, 2, 28, 28, 17, 48, 124, kSequencePointKind_StepOut, 0, 37 },
	{ 140383, 2, 28, 28, 17, 48, 129, kSequencePointKind_StepOut, 0, 38 },
	{ 140383, 2, 28, 28, 0, 0, 136, kSequencePointKind_Normal, 0, 39 },
	{ 140383, 2, 29, 29, 17, 18, 140, kSequencePointKind_Normal, 0, 40 },
	{ 140383, 2, 30, 30, 21, 50, 141, kSequencePointKind_Normal, 0, 41 },
	{ 140383, 2, 30, 30, 21, 50, 142, kSequencePointKind_StepOut, 0, 42 },
	{ 140383, 2, 31, 31, 17, 18, 148, kSequencePointKind_Normal, 0, 43 },
	{ 140383, 2, 32, 32, 13, 14, 149, kSequencePointKind_Normal, 0, 44 },
	{ 140383, 2, 32, 32, 0, 0, 150, kSequencePointKind_Normal, 0, 45 },
	{ 140383, 2, 34, 34, 13, 14, 155, kSequencePointKind_Normal, 0, 46 },
	{ 140383, 2, 35, 35, 17, 126, 156, kSequencePointKind_Normal, 0, 47 },
	{ 140383, 2, 35, 35, 17, 126, 162, kSequencePointKind_StepOut, 0, 48 },
	{ 140383, 2, 35, 35, 17, 126, 170, kSequencePointKind_StepOut, 0, 49 },
	{ 140383, 2, 35, 35, 17, 126, 196, kSequencePointKind_StepOut, 0, 50 },
	{ 140383, 2, 35, 35, 17, 126, 201, kSequencePointKind_StepOut, 0, 51 },
	{ 140383, 2, 35, 35, 17, 126, 206, kSequencePointKind_StepOut, 0, 52 },
	{ 140383, 2, 38, 38, 17, 45, 212, kSequencePointKind_Normal, 0, 53 },
	{ 140383, 2, 38, 38, 17, 45, 213, kSequencePointKind_StepOut, 0, 54 },
	{ 140383, 2, 38, 38, 0, 0, 220, kSequencePointKind_Normal, 0, 55 },
	{ 140383, 2, 39, 39, 17, 18, 224, kSequencePointKind_Normal, 0, 56 },
	{ 140383, 2, 40, 40, 21, 61, 225, kSequencePointKind_Normal, 0, 57 },
	{ 140383, 2, 40, 40, 21, 61, 230, kSequencePointKind_StepOut, 0, 58 },
	{ 140383, 2, 40, 40, 21, 61, 235, kSequencePointKind_StepOut, 0, 59 },
	{ 140383, 2, 40, 40, 0, 0, 242, kSequencePointKind_Normal, 0, 60 },
	{ 140383, 2, 41, 41, 21, 22, 246, kSequencePointKind_Normal, 0, 61 },
	{ 140383, 2, 42, 42, 25, 64, 247, kSequencePointKind_Normal, 0, 62 },
	{ 140383, 2, 42, 42, 25, 64, 248, kSequencePointKind_StepOut, 0, 63 },
	{ 140383, 2, 42, 42, 0, 0, 258, kSequencePointKind_Normal, 0, 64 },
	{ 140383, 2, 43, 43, 25, 26, 262, kSequencePointKind_Normal, 0, 65 },
	{ 140383, 2, 45, 45, 29, 126, 263, kSequencePointKind_Normal, 0, 66 },
	{ 140383, 2, 45, 45, 29, 126, 264, kSequencePointKind_StepOut, 0, 67 },
	{ 140383, 2, 45, 45, 29, 126, 276, kSequencePointKind_StepOut, 0, 68 },
	{ 140383, 2, 45, 45, 0, 0, 283, kSequencePointKind_Normal, 0, 69 },
	{ 140383, 2, 46, 46, 29, 30, 287, kSequencePointKind_Normal, 0, 70 },
	{ 140383, 2, 48, 48, 33, 75, 288, kSequencePointKind_Normal, 0, 71 },
	{ 140383, 2, 48, 48, 33, 75, 290, kSequencePointKind_StepOut, 0, 72 },
	{ 140383, 2, 49, 49, 29, 30, 296, kSequencePointKind_Normal, 0, 73 },
	{ 140383, 2, 50, 50, 25, 26, 297, kSequencePointKind_Normal, 0, 74 },
	{ 140383, 2, 51, 51, 21, 22, 298, kSequencePointKind_Normal, 0, 75 },
	{ 140383, 2, 52, 52, 17, 18, 299, kSequencePointKind_Normal, 0, 76 },
	{ 140383, 2, 53, 53, 13, 14, 300, kSequencePointKind_Normal, 0, 77 },
	{ 140383, 2, 55, 55, 13, 33, 301, kSequencePointKind_Normal, 0, 78 },
	{ 140383, 2, 55, 55, 13, 33, 301, kSequencePointKind_StepOut, 0, 79 },
	{ 140383, 2, 56, 56, 9, 10, 307, kSequencePointKind_Normal, 0, 80 },
	{ 140385, 0, 0, 0, 0, 0, -1, kSequencePointKind_Normal, 0, 81 },
	{ 140385, 0, 0, 0, 0, 0, 16777215, kSequencePointKind_Normal, 0, 82 },
	{ 140385, 0, -1, -1, -1, -1, 1, kSequencePointKind_StepOut, 0, 83 },
	{ 140385, 0, -1, -1, -1, -1, 20, kSequencePointKind_StepOut, 0, 84 },
	{ 140385, 0, -1, -1, -1, -1, 33, kSequencePointKind_StepOut, 0, 85 },
	{ 140385, 0, -1, -1, -1, -1, 69, kSequencePointKind_StepOut, 0, 86 },
	{ 140385, 0, -1, -1, -1, -1, 83, kSequencePointKind_StepOut, 0, 87 },
	{ 140385, 0, -1, -1, -1, -1, 95, kSequencePointKind_StepOut, 0, 88 },
	{ 140385, 0, -1, -1, -1, -1, 104, kSequencePointKind_StepOut, 0, 89 },
	{ 140385, 0, -1, -1, -1, -1, 156, kSequencePointKind_StepOut, 0, 90 },
	{ 140385, 0, -1, -1, -1, -1, 179, kSequencePointKind_StepOut, 0, 91 },
	{ 140385, 0, -1, -1, -1, -1, 208, kSequencePointKind_StepOut, 0, 92 },
	{ 140385, 0, -1, -1, -1, -1, 236, kSequencePointKind_StepOut, 0, 93 },
	{ 140385, 0, -1, -1, -1, -1, 250, kSequencePointKind_StepOut, 0, 94 },
	{ 140385, 3, 20, 20, 9, 10, 273, kSequencePointKind_Normal, 0, 95 },
	{ 140385, 3, 21, 21, 13, 50, 274, kSequencePointKind_Normal, 0, 96 },
	{ 140385, 3, 21, 21, 13, 50, 275, kSequencePointKind_StepOut, 0, 97 },
	{ 140385, 3, 21, 21, 13, 50, 280, kSequencePointKind_StepOut, 0, 98 },
	{ 140385, 3, 22, 22, 13, 63, 286, kSequencePointKind_Normal, 0, 99 },
	{ 140385, 3, 22, 22, 13, 63, 287, kSequencePointKind_StepOut, 0, 100 },
	{ 140385, 3, 22, 22, 13, 63, 292, kSequencePointKind_StepOut, 0, 101 },
	{ 140385, 3, 22, 22, 13, 63, 297, kSequencePointKind_StepOut, 0, 102 },
	{ 140385, 3, 23, 23, 13, 50, 303, kSequencePointKind_Normal, 0, 103 },
	{ 140385, 3, 23, 23, 13, 50, 304, kSequencePointKind_StepOut, 0, 104 },
	{ 140385, 3, 23, 23, 13, 50, 309, kSequencePointKind_StepOut, 0, 105 },
	{ 140385, 3, 24, 24, 13, 185, 315, kSequencePointKind_Normal, 0, 106 },
	{ 140385, 3, 24, 24, 13, 185, 345, kSequencePointKind_StepOut, 0, 107 },
	{ 140385, 3, 24, 24, 13, 185, 390, kSequencePointKind_StepOut, 0, 108 },
	{ 140385, 3, 24, 24, 13, 185, 395, kSequencePointKind_StepOut, 0, 109 },
	{ 140385, 3, 25, 25, 9, 10, 401, kSequencePointKind_Normal, 0, 110 },
	{ 140386, 0, 0, 0, 0, 0, -1, kSequencePointKind_Normal, 0, 111 },
	{ 140386, 0, 0, 0, 0, 0, 16777215, kSequencePointKind_Normal, 0, 112 },
	{ 140386, 3, 28, 28, 9, 10, 0, kSequencePointKind_Normal, 0, 113 },
	{ 140386, 3, 29, 29, 13, 82, 1, kSequencePointKind_Normal, 0, 114 },
	{ 140386, 3, 29, 29, 13, 82, 11, kSequencePointKind_StepOut, 0, 115 },
	{ 140386, 3, 29, 29, 13, 82, 26, kSequencePointKind_StepOut, 0, 116 },
	{ 140386, 3, 29, 29, 13, 82, 36, kSequencePointKind_StepOut, 0, 117 },
	{ 140386, 3, 30, 30, 9, 10, 44, kSequencePointKind_Normal, 0, 118 },
};
#else
extern Il2CppSequencePoint g_sequencePointsBootstrap[];
Il2CppSequencePoint g_sequencePointsBootstrap[1] = { { 0, 0, 0, 0, 0, 0, 0, kSequencePointKind_Normal, 0, 0, } };
#endif
#if IL2CPP_MONO_DEBUGGER
static const Il2CppCatchPoint g_catchPoints[1] = { { 0, 0, 0, 0, } };
#else
static const Il2CppCatchPoint g_catchPoints[1] = { { 0, 0, 0, 0, } };
#endif
#if IL2CPP_MONO_DEBUGGER
static const Il2CppSequencePointSourceFile g_sequencePointSourceFiles[] = {
{ "", { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0} },
{ "C:\\Users\\user\\My project (2)\\Unity.SourceGenerators\\Unity.MonoScriptGenerator.MonoScriptInfoGenerator\\AssemblyMonoScriptTypes.generated.cs", { 52, 224, 44, 194, 252, 123, 155, 243, 145, 169, 20, 247, 14, 106, 147, 97} },
{ "C:\\Users\\user\\My project (2)\\Assets\\Samples\\Netcode for GameObjects\\2.10.0\\Bootstrap\\Scripts\\BootstrapManager.cs", { 73, 29, 212, 58, 87, 95, 205, 110, 19, 14, 93, 2, 52, 207, 140, 233} },
{ "C:\\Users\\user\\My project (2)\\Assets\\Samples\\Netcode for GameObjects\\2.10.0\\Bootstrap\\Scripts\\BootstrapPlayer.cs", { 62, 143, 146, 223, 199, 130, 104, 162, 182, 39, 8, 43, 66, 176, 12, 210} },
};
#else
static const Il2CppSequencePointSourceFile g_sequencePointSourceFiles[1] = { NULL, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
#endif
#if IL2CPP_MONO_DEBUGGER
static const Il2CppTypeSourceFilePair g_typeSourceFiles[3] = 
{
	{ 18258, 1 },
	{ 18259, 2 },
	{ 18260, 3 },
};
#else
static const Il2CppTypeSourceFilePair g_typeSourceFiles[1] = { { 0, 0 } };
#endif
#if IL2CPP_MONO_DEBUGGER
static const Il2CppMethodScope g_methodScopes[5] = 
{
	{ 0, 92 },
	{ 0, 308 },
	{ 262, 298 },
	{ 273, 402 },
	{ 0, 46 },
};
#else
static const Il2CppMethodScope g_methodScopes[1] = { { 0, 0 } };
#endif
#if IL2CPP_MONO_DEBUGGER
static const Il2CppMethodHeaderInfo g_methodHeaderInfos[12] = 
{
	{ 92, 0, 1 },
	{ 0, 0, 0 },
	{ 308, 1, 2 },
	{ 0, 0, 0 },
	{ 402, 3, 1 },
	{ 46, 4, 1 },
	{ 0, 0, 0 },
	{ 0, 0, 0 },
	{ 0, 0, 0 },
	{ 0, 0, 0 },
	{ 0, 0, 0 },
	{ 0, 0, 0 },
};
#else
static const Il2CppMethodHeaderInfo g_methodHeaderInfos[1] = { { 0, 0, 0 } };
#endif
IL2CPP_EXTERN_C const Il2CppDebuggerMetadataRegistration g_DebuggerMetadataRegistrationBootstrap;
const Il2CppDebuggerMetadataRegistration g_DebuggerMetadataRegistrationBootstrap = 
{
	(Il2CppMethodExecutionContextInfo*)g_methodExecutionContextInfos,
	(Il2CppMethodExecutionContextInfoIndex*)g_methodExecutionContextInfoIndexes,
	(Il2CppMethodScope*)g_methodScopes,
	(Il2CppMethodHeaderInfo*)g_methodHeaderInfos,
	(Il2CppSequencePointSourceFile*)g_sequencePointSourceFiles,
	119,
	(Il2CppSequencePoint*)g_sequencePointsBootstrap,
	0,
	(Il2CppCatchPoint*)g_catchPoints,
	3,
	(Il2CppTypeSourceFilePair*)g_typeSourceFiles,
	(const char**)g_methodExecutionContextInfoStrings,
};
