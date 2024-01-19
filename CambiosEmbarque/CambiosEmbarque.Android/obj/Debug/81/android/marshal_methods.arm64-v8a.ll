; ModuleID = 'obj\Debug\81\android\marshal_methods.arm64-v8a.ll'
source_filename = "obj\Debug\81\android\marshal_methods.arm64-v8a.ll"
target datalayout = "e-m:e-i8:8:32-i16:16:32-i64:64-i128:128-n32:64-S128"
target triple = "aarch64-unknown-linux-android"


%struct.MonoImage = type opaque

%struct.MonoClass = type opaque

%struct.MarshalMethodsManagedClass = type {
	i32,; uint32_t token
	%struct.MonoClass*; MonoClass* klass
}

%struct.MarshalMethodName = type {
	i64,; uint64_t id
	i8*; char* name
}

%class._JNIEnv = type opaque

%class._jobject = type {
	i8; uint8_t b
}

%class._jclass = type {
	i8; uint8_t b
}

%class._jstring = type {
	i8; uint8_t b
}

%class._jthrowable = type {
	i8; uint8_t b
}

%class._jarray = type {
	i8; uint8_t b
}

%class._jobjectArray = type {
	i8; uint8_t b
}

%class._jbooleanArray = type {
	i8; uint8_t b
}

%class._jbyteArray = type {
	i8; uint8_t b
}

%class._jcharArray = type {
	i8; uint8_t b
}

%class._jshortArray = type {
	i8; uint8_t b
}

%class._jintArray = type {
	i8; uint8_t b
}

%class._jlongArray = type {
	i8; uint8_t b
}

%class._jfloatArray = type {
	i8; uint8_t b
}

%class._jdoubleArray = type {
	i8; uint8_t b
}

; assembly_image_cache
@assembly_image_cache = local_unnamed_addr global [0 x %struct.MonoImage*] zeroinitializer, align 8
; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = local_unnamed_addr constant [62 x i64] [
	i64 120698629574877762, ; 0: Mono.Android => 0x1accec39cafe242 => 6
	i64 156291772854606065, ; 1: I18N.West => 0x22b428a125098f1 => 30
	i64 702024105029695270, ; 2: System.Drawing.Common => 0x9be17343c0e7726 => 27
	i64 940822596282819491, ; 3: System.Transactions => 0xd0e792aa81923a3 => 26
	i64 996343623809489702, ; 4: Xamarin.Forms.Platform => 0xdd3b93f3b63db26 => 21
	i64 1000557547492888992, ; 5: Mono.Security.dll => 0xde2b1c9cba651a0 => 28
	i64 1342439039765371018, ; 6: Xamarin.Android.Support.Fragment => 0x12a14d31b1d4d88a => 16
	i64 1425944114962822056, ; 7: System.Runtime.Serialization.dll => 0x13c9f89e19eaf3a8 => 2
	i64 1744702963312407042, ; 8: Xamarin.Android.Support.v7.AppCompat => 0x18366e19eeceb202 => 17
	i64 2592350477072141967, ; 9: System.Xml.dll => 0x23f9e10627330e8f => 10
	i64 2624866290265602282, ; 10: mscorlib.dll => 0x246d65fbde2db8ea => 7
	i64 2960931600190307745, ; 11: Xamarin.Forms.Core => 0x2917579a49927da1 => 19
	i64 3531994851595924923, ; 12: System.Numerics => 0x31042a9aade235bb => 25
	i64 3572576518857361216, ; 13: I18N => 0x3194576a63650740 => 29
	i64 4525561845656915374, ; 14: System.ServiceModel.Internals => 0x3ece06856b710dae => 23
	i64 5142919913060024034, ; 15: Xamarin.Forms.Platform.Android.dll => 0x475f52699e39bee2 => 20
	i64 5398069113008343190, ; 16: I18N.West.dll => 0x4ae9cb4211dec896 => 30
	i64 5507995362134886206, ; 17: System.Core.dll => 0x4c705499688c873e => 8
	i64 6085203216496545422, ; 18: Xamarin.Forms.Platform.dll => 0x5472fc15a9574e8e => 21
	i64 6086316965293125504, ; 19: FormsViewGroup.dll => 0x5476f10882baef80 => 4
	i64 7635363394907363464, ; 20: Xamarin.Forms.Core.dll => 0x69f6428dc4795888 => 19
	i64 7654504624184590948, ; 21: System.Net.Http => 0x6a3a4366801b8264 => 1
	i64 7820441508502274321, ; 22: System.Data => 0x6c87ca1e14ff8111 => 24
	i64 7879037620440914030, ; 23: Xamarin.Android.Support.v7.AppCompat.dll => 0x6d57f6f88a51d86e => 17
	i64 8167236081217502503, ; 24: Java.Interop.dll => 0x7157d9f1a9b8fd27 => 5
	i64 8265650852517415196, ; 25: I18N.dll => 0x72b57da835b4891c => 29
	i64 8626175481042262068, ; 26: Java.Interop => 0x77b654e585b55834 => 5
	i64 9475595603812259686, ; 27: Xamarin.Android.Support.Design => 0x838013ff707b9766 => 15
	i64 9662334977499516867, ; 28: System.Numerics.dll => 0x8617827802b0cfc3 => 25
	i64 9808709177481450983, ; 29: Mono.Android.dll => 0x881f890734e555e7 => 6
	i64 9834056768316610435, ; 30: System.Transactions.dll => 0x8879968718899783 => 26
	i64 9866412715007501892, ; 31: Xamarin.Android.Arch.Lifecycle.Common.dll => 0x88ec8a16fd6b6644 => 11
	i64 9998632235833408227, ; 32: Mono.Security => 0x8ac2470b209ebae3 => 28
	i64 10038780035334861115, ; 33: System.Net.Http.dll => 0x8b50e941206af13b => 1
	i64 10391997902924734469, ; 34: CambiosEmbarque.Android.dll => 0x9037cb08966ee005 => 0
	i64 11023048688141570732, ; 35: System.Core => 0x98f9bc61168392ac => 8
	i64 11037814507248023548, ; 36: System.Xml => 0x992e31d0412bf7fc => 10
	i64 11376461258732682436, ; 37: Xamarin.Android.Support.Compat => 0x9de14f3d5fc13cc4 => 12
	i64 11395105072750394936, ; 38: Xamarin.Android.Support.v7.CardView => 0x9e238bb09789fe38 => 18
	i64 12414299427252656003, ; 39: Xamarin.Android.Support.Compat.dll => 0xac48738e28bad783 => 12
	i64 12559163541709922900, ; 40: Xamarin.Android.Support.v7.CardView.dll => 0xae4b1cb32ba82254 => 18
	i64 12952608645614506925, ; 41: Xamarin.Android.Support.Core.Utils => 0xb3c0e8eff48193ad => 14
	i64 12963446364377008305, ; 42: System.Drawing.Common.dll => 0xb3e769c8fd8548b1 => 27
	i64 13358059602087096138, ; 43: Xamarin.Android.Support.Fragment.dll => 0xb9615c6f1ee5af4a => 16
	i64 13370592475155966277, ; 44: System.Runtime.Serialization => 0xb98de304062ea945 => 2
	i64 13647894001087880694, ; 45: System.Data.dll => 0xbd670f48cb071df6 => 24
	i64 13967638549803255703, ; 46: Xamarin.Forms.Platform.Android => 0xc1d70541e0134797 => 20
	i64 14373511135891791624, ; 47: CambiosEmbarque.Android => 0xc778f83955f39f08 => 0
	i64 14400856865250966808, ; 48: Xamarin.Android.Support.Core.UI => 0xc7da1f051a877d18 => 13
	i64 15609085926864131306, ; 49: System.dll => 0xd89e9cf3334914ea => 9
	i64 15810740023422282496, ; 50: Xamarin.Forms.Xaml => 0xdb6b08484c22eb00 => 22
	i64 16154507427712707110, ; 51: System => 0xe03056ea4e39aa26 => 9
	i64 16833383113903931215, ; 52: mscorlib => 0xe99c30c1484d7f4f => 7
	i64 16857430145648254351, ; 53: EmbarquesADM.dll => 0xe9f19f6729535d8f => 3
	i64 16932527889823454152, ; 54: Xamarin.Android.Support.Core.Utils.dll => 0xeafc6c67465253c8 => 14
	i64 17428701562824544279, ; 55: Xamarin.Android.Support.Core.UI.dll => 0xf1df2fbaec73d017 => 13
	i64 17760961058993581169, ; 56: Xamarin.Android.Arch.Lifecycle.Common => 0xf67b9bfb46dbac71 => 11
	i64 17882897186074144999, ; 57: FormsViewGroup => 0xf82cd03e3ac830e7 => 4
	i64 17892495832318972303, ; 58: Xamarin.Forms.Xaml.dll => 0xf84eea293687918f => 22
	i64 17936749993673010118, ; 59: Xamarin.Android.Support.Design.dll => 0xf8ec231615deabc6 => 15
	i64 18129453464017766560, ; 60: System.ServiceModel.Internals.dll => 0xfb98c1df1ec108a0 => 23
	i64 18334172926391105909 ; 61: EmbarquesADM => 0xfe70112350203975 => 3
], align 8
@assembly_image_cache_indices = local_unnamed_addr constant [62 x i32] [
	i32 6, i32 30, i32 27, i32 26, i32 21, i32 28, i32 16, i32 2, ; 0..7
	i32 17, i32 10, i32 7, i32 19, i32 25, i32 29, i32 23, i32 20, ; 8..15
	i32 30, i32 8, i32 21, i32 4, i32 19, i32 1, i32 24, i32 17, ; 16..23
	i32 5, i32 29, i32 5, i32 15, i32 25, i32 6, i32 26, i32 11, ; 24..31
	i32 28, i32 1, i32 0, i32 8, i32 10, i32 12, i32 18, i32 12, ; 32..39
	i32 18, i32 14, i32 27, i32 16, i32 2, i32 24, i32 20, i32 0, ; 40..47
	i32 13, i32 9, i32 22, i32 9, i32 7, i32 3, i32 14, i32 13, ; 48..55
	i32 11, i32 4, i32 22, i32 15, i32 23, i32 3 ; 56..61
], align 4

@marshal_methods_number_of_classes = local_unnamed_addr constant i32 0, align 4

; marshal_methods_class_cache
@marshal_methods_class_cache = global [0 x %struct.MarshalMethodsManagedClass] [
], align 8; end of 'marshal_methods_class_cache' array


@get_function_pointer = internal unnamed_addr global void (i32, i32, i32, i8**)* null, align 8

; Function attributes: "frame-pointer"="non-leaf" "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" "target-cpu"="generic" "target-features"="+neon,+outline-atomics" uwtable willreturn writeonly
define void @xamarin_app_init (void (i32, i32, i32, i8**)* %fn) local_unnamed_addr #0
{
	store void (i32, i32, i32, i8**)* %fn, void (i32, i32, i32, i8**)** @get_function_pointer, align 8
	ret void
}

; Names of classes in which marshal methods reside
@mm_class_names = local_unnamed_addr constant [0 x i8*] zeroinitializer, align 8
@__MarshalMethodName_name.0 = internal constant [1 x i8] c"\00", align 1

; mm_method_names
@mm_method_names = local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	; 0
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		i8* getelementptr inbounds ([1 x i8], [1 x i8]* @__MarshalMethodName_name.0, i32 0, i32 0); name
	}
], align 8; end of 'mm_method_names' array


attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" uwtable willreturn writeonly "frame-pointer"="non-leaf" "target-cpu"="generic" "target-features"="+neon,+outline-atomics" }
attributes #1 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" uwtable "frame-pointer"="non-leaf" "target-cpu"="generic" "target-features"="+neon,+outline-atomics" }
attributes #2 = { nounwind }

!llvm.module.flags = !{!0, !1, !2, !3, !4, !5}
!llvm.ident = !{!6}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{i32 1, !"branch-target-enforcement", i32 0}
!3 = !{i32 1, !"sign-return-address", i32 0}
!4 = !{i32 1, !"sign-return-address-all", i32 0}
!5 = !{i32 1, !"sign-return-address-with-bkey", i32 0}
!6 = !{!"Xamarin.Android remotes/origin/d17-5 @ 45b0e144f73b2c8747d8b5ec8cbd3b55beca67f0"}
!llvm.linker.options = !{}
