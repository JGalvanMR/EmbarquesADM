; ModuleID = 'obj\Debug\81\android\marshal_methods.x86.ll'
source_filename = "obj\Debug\81\android\marshal_methods.x86.ll"
target datalayout = "e-m:e-p:32:32-p270:32:32-p271:32:32-p272:64:64-f64:32:64-f80:32-n8:16:32-S128"
target triple = "i686-unknown-linux-android"


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
@assembly_image_cache = local_unnamed_addr global [0 x %struct.MonoImage*] zeroinitializer, align 4
; Each entry maps hash of an assembly name to an index into the `assembly_image_cache` array
@assembly_image_cache_hashes = local_unnamed_addr constant [62 x i32] [
	i32 48238590, ; 0: EmbarquesADM.dll => 0x2e00ffe => 3
	i32 57263871, ; 1: Xamarin.Forms.Core.dll => 0x369c6ff => 19
	i32 166922606, ; 2: Xamarin.Android.Support.Compat.dll => 0x9f3096e => 12
	i32 321597661, ; 3: System.Numerics => 0x132b30dd => 25
	i32 389971796, ; 4: Xamarin.Android.Support.Core.UI => 0x173e7f54 => 13
	i32 465846621, ; 5: mscorlib => 0x1bc4415d => 7
	i32 469710990, ; 6: System.dll => 0x1bff388e => 9
	i32 514659665, ; 7: Xamarin.Android.Support.Compat => 0x1ead1551 => 12
	i32 526420162, ; 8: System.Transactions.dll => 0x1f6088c2 => 26
	i32 539750087, ; 9: Xamarin.Android.Support.Design => 0x202beec7 => 15
	i32 725851412, ; 10: I18N.West.dll => 0x2b439d14 => 30
	i32 809851609, ; 11: System.Drawing.Common.dll => 0x30455ad9 => 27
	i32 974778368, ; 12: FormsViewGroup.dll => 0x3a19f000 => 4
	i32 1042160112, ; 13: Xamarin.Forms.Platform.dll => 0x3e1e19f0 => 21
	i32 1098259244, ; 14: System => 0x41761b2c => 9
	i32 1359785034, ; 15: Xamarin.Android.Support.Design.dll => 0x510cac4a => 15
	i32 1365406463, ; 16: System.ServiceModel.Internals.dll => 0x516272ff => 23
	i32 1445445088, ; 17: Xamarin.Android.Support.Fragment => 0x5627bde0 => 16
	i32 1460219004, ; 18: Xamarin.Forms.Xaml => 0x57092c7c => 22
	i32 1574652163, ; 19: Xamarin.Android.Support.Core.Utils.dll => 0x5ddb4903 => 14
	i32 1592978981, ; 20: System.Runtime.Serialization.dll => 0x5ef2ee25 => 2
	i32 1639515021, ; 21: System.Net.Http.dll => 0x61b9038d => 1
	i32 1776026572, ; 22: System.Core.dll => 0x69dc03cc => 8
	i32 1878053835, ; 23: Xamarin.Forms.Xaml.dll => 0x6ff0d3cb => 22
	i32 2067863569, ; 24: I18N.dll => 0x7b411811 => 29
	i32 2104130521, ; 25: EmbarquesADM => 0x7d6a7bd9 => 3
	i32 2126786730, ; 26: Xamarin.Forms.Platform.Android => 0x7ec430aa => 20
	i32 2166116741, ; 27: Xamarin.Android.Support.Core.Utils => 0x811c5185 => 14
	i32 2201231467, ; 28: System.Net.Http => 0x8334206b => 1
	i32 2330457430, ; 29: Xamarin.Android.Support.Core.UI.dll => 0x8ae7f556 => 13
	i32 2373288475, ; 30: Xamarin.Android.Support.Fragment.dll => 0x8d75821b => 16
	i32 2475788418, ; 31: Java.Interop.dll => 0x93918882 => 5
	i32 2581274016, ; 32: I18N.West => 0x99db1da0 => 30
	i32 2597279867, ; 33: CambiosEmbarque.Android => 0x9acf587b => 0
	i32 2766581644, ; 34: Xamarin.Forms.Core => 0xa4e6af8c => 19
	i32 2819470561, ; 35: System.Xml.dll => 0xa80db4e1 => 10
	i32 2905242038, ; 36: mscorlib.dll => 0xad2a79b6 => 7
	i32 3044182254, ; 37: FormsViewGroup => 0xb57288ee => 4
	i32 3068715062, ; 38: Xamarin.Android.Arch.Lifecycle.Common => 0xb6e8e036 => 11
	i32 3111772706, ; 39: System.Runtime.Serialization => 0xb979e222 => 2
	i32 3204380047, ; 40: System.Data.dll => 0xbefef58f => 24
	i32 3247949154, ; 41: Mono.Security => 0xc197c562 => 28
	i32 3317144872, ; 42: System.Data => 0xc5b79d28 => 24
	i32 3366347497, ; 43: Java.Interop => 0xc8a662e9 => 5
	i32 3393046670, ; 44: CambiosEmbarque.Android.dll => 0xca3dc88e => 0
	i32 3404865022, ; 45: System.ServiceModel.Internals => 0xcaf21dfe => 23
	i32 3429136800, ; 46: System.Xml => 0xcc6479a0 => 10
	i32 3476120550, ; 47: Mono.Android => 0xcf3163e6 => 6
	i32 3486566296, ; 48: System.Transactions => 0xcfd0c798 => 26
	i32 3498942916, ; 49: Xamarin.Android.Support.v7.CardView.dll => 0xd08da1c4 => 18
	i32 3536029504, ; 50: Xamarin.Forms.Platform.Android.dll => 0xd2c38740 => 20
	i32 3579244613, ; 51: I18N => 0xd556f045 => 29
	i32 3632359727, ; 52: Xamarin.Forms.Platform => 0xd881692f => 21
	i32 3672681054, ; 53: Mono.Android.dll => 0xdae8aa5e => 6
	i32 3678221644, ; 54: Xamarin.Android.Support.v7.AppCompat => 0xdb3d354c => 17
	i32 3681174138, ; 55: Xamarin.Android.Arch.Lifecycle.Common.dll => 0xdb6a427a => 11
	i32 3689375977, ; 56: System.Drawing.Common => 0xdbe768e9 => 27
	i32 3829621856, ; 57: System.Numerics.dll => 0xe4436460 => 25
	i32 3883175360, ; 58: Xamarin.Android.Support.v7.AppCompat.dll => 0xe7748dc0 => 17
	i32 4105002889, ; 59: Mono.Security.dll => 0xf4ad5f89 => 28
	i32 4151237749, ; 60: System.Core => 0xf76edc75 => 8
	i32 4219003402 ; 61: Xamarin.Android.Support.v7.CardView => 0xfb78e20a => 18
], align 4
@assembly_image_cache_indices = local_unnamed_addr constant [62 x i32] [
	i32 3, i32 19, i32 12, i32 25, i32 13, i32 7, i32 9, i32 12, ; 0..7
	i32 26, i32 15, i32 30, i32 27, i32 4, i32 21, i32 9, i32 15, ; 8..15
	i32 23, i32 16, i32 22, i32 14, i32 2, i32 1, i32 8, i32 22, ; 16..23
	i32 29, i32 3, i32 20, i32 14, i32 1, i32 13, i32 16, i32 5, ; 24..31
	i32 30, i32 0, i32 19, i32 10, i32 7, i32 4, i32 11, i32 2, ; 32..39
	i32 24, i32 28, i32 24, i32 5, i32 0, i32 23, i32 10, i32 6, ; 40..47
	i32 26, i32 18, i32 20, i32 29, i32 21, i32 6, i32 17, i32 11, ; 48..55
	i32 27, i32 25, i32 17, i32 28, i32 8, i32 18 ; 56..61
], align 4

@marshal_methods_number_of_classes = local_unnamed_addr constant i32 0, align 4

; marshal_methods_class_cache
@marshal_methods_class_cache = global [0 x %struct.MarshalMethodsManagedClass] [
], align 4; end of 'marshal_methods_class_cache' array


@get_function_pointer = internal unnamed_addr global void (i32, i32, i32, i8**)* null, align 4

; Function attributes: "frame-pointer"="none" "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" "stackrealign" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" uwtable willreturn writeonly
define void @xamarin_app_init (void (i32, i32, i32, i8**)* %fn) local_unnamed_addr #0
{
	store void (i32, i32, i32, i8**)* %fn, void (i32, i32, i32, i8**)** @get_function_pointer, align 4
	ret void
}

; Names of classes in which marshal methods reside
@mm_class_names = local_unnamed_addr constant [0 x i8*] zeroinitializer, align 4
@__MarshalMethodName_name.0 = internal constant [1 x i8] c"\00", align 1

; mm_method_names
@mm_method_names = local_unnamed_addr constant [1 x %struct.MarshalMethodName] [
	; 0
	%struct.MarshalMethodName {
		i64 0, ; id 0x0; name: 
		i8* getelementptr inbounds ([1 x i8], [1 x i8]* @__MarshalMethodName_name.0, i32 0, i32 0); name
	}
], align 8; end of 'mm_method_names' array


attributes #0 = { "min-legal-vector-width"="0" mustprogress nofree norecurse nosync "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" uwtable willreturn writeonly "frame-pointer"="none" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" "stackrealign" }
attributes #1 = { "min-legal-vector-width"="0" mustprogress "no-trapping-math"="true" nounwind sspstrong "stack-protector-buffer-size"="8" uwtable "frame-pointer"="none" "target-cpu"="i686" "target-features"="+cx8,+mmx,+sse,+sse2,+sse3,+ssse3,+x87" "tune-cpu"="generic" "stackrealign" }
attributes #2 = { nounwind }

!llvm.module.flags = !{!0, !1, !2}
!llvm.ident = !{!3}
!0 = !{i32 1, !"wchar_size", i32 4}
!1 = !{i32 7, !"PIC Level", i32 2}
!2 = !{i32 1, !"NumRegisterParameters", i32 0}
!3 = !{!"Xamarin.Android remotes/origin/d17-5 @ 45b0e144f73b2c8747d8b5ec8cbd3b55beca67f0"}
!llvm.linker.options = !{}
