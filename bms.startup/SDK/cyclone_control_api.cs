using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace visual_sap_control
{
    public static class cyclone_control_api
    {
        public const int max_number_of_cyclones = 100;

        /* CATEGORY NAMES AND THEIR PROPERTIES
        A list of properties supported by the Cyclone can
        be retrieved with the getPropertyList() routine */
        public const string CycloneProperties = "cycloneProperties";
          public const string selectCyclonePropertyType = "cycloneType";
          public const string selectCyclonePropertyFirmwareVersion = "cycloneFirmwareVersion";
          public const string selectCyclonePropertyCycloneLogicVersion = "cycloneLogicVersion";
          public const string selectCyclonePropertyName = "cycloneName";
          public const string selectCyclonePropertyNumberOfExternalImages = "numberOfExternalImages";
          public const string selectCyclonePropertyNumberOfInternalImages = "numberOfInternalImages";
          public const string selectCyclonePropertyNumberOfImages = "totalnumberofimages";
          public const string currentImageSelected = "currentImageSelected";
        public const string NetworkProperties = "networkProperties";
          public const string selectNetworkPropertyCycloneIPAddress = "cycloneIPAddress";
          public const string selectNetworkPropertyCycloneNetmask = "cycloneNetworkMask";
          public const string selectNetworkPropertyCycloneGateway = "cycloneNetworkGateway";
          public const string selectNetworkPropertyCycloneDNS = "cycloneDNSAddress";
        public const string ImageProperties = "imageProperties";
          public const string selectImagePropertyName = "imageName";
          public const string selectImagePropertyMediaType = "mediaType";
          public const string selectImagePropertyUniqueId = "imageUniqueId";
          public const string selectImagePropertyCRC32 = "imageCRC32";
          public const string selectImagePropertyVoltageSettings = "imageVoltageSettings";
          public const string selectImagePropertyFirstObjectCrc = "imageFirstObjectCrc";
          public const string selectImagePropertyFirstDeviceCrc = "imageFirstDeviceCrc";
          public const string selectImagePropertySerialNumberCount = "imageSerialNumberCount";
          public const string selectImagePropertyGetSerialNumber = "imageSerialNumber"; /* Append index in decimal format, e.g. 'imageSerialNumber1', 'imageSerialNumber2' */

        //MEDIA TYPE VALUES
        public const byte MEDIA_INTERNAL = 1;
        public const byte MEDIA_EXTERNAL = 2;

        //CYCLONE SPECIAL FEATURES 
        public const UInt32 PE_SET_FIRMWARE_UPDATE_PRINTF_CALLBACK = 0x58006001;
        public const UInt32 PE_CYCLONE_SDK_SET_FIRMWARE_UPDATE_MODE = 0x58006002;
        public const UInt32 PE_CYCLONE_SDK_ENABLE_DEBUG_OUT_FILE = 0x58006006;

        public const UInt32 CYCLONE_GET_IMAGE_DESCRIPTION_FROM_FILE = 0xA5001001;
        public const UInt32 CYCLONE_GET_IMAGE_CRC32_FROM_FILE = 0xA5001002;
        public const UInt32 CYCLONE_GET_IMAGE_SETTINGS_FROM_FILE = 0xA5001003;
        public const UInt32 CYCLONE_GET_IMAGE_COMMMAND_LINE_PARAMS_FROM_FILE = 0xA5001004;
        public const UInt32 CYCLONE_GET_IMAGE_SCRIPT_FILE_FROM_FILE = 0xA5001005;

        public const UInt32 PE_CYCLONE_GET_CYCLONE_SCREEN_BITMAP_BUFFER = 0xA5001101;
        public const UInt32 PE_CYCLONE_SEND_DISPLAY_TOUCH = 0xA5001102;
        public const UInt32 PE_CYCLONE_DOES_DISPLAY_NEED_UPDATE = 0xA5001103;

        public const UInt32 CYCLONE_TOGGLE_POWER_NO_DEBUG = 0xA5002001;
        public const UInt32 CYCLONE_SET_ACTIVE_SECURITY_CODE = 0xA5002002;

        //CYCLONE PORT TYPE AND IDENTIFIER TYPES
        public const UInt32 CyclonePortType_USB = 5;
        public const UInt32 CyclonePortType_Ethernet = 6;
        public const UInt32 CyclonePortType_Serial = 7;

        public const UInt32 CycloneInformation_IP_Address = 1;
        public const UInt32 CycloneInformation_Name = 2;
        public const UInt32 CycloneInformation_Generic_Port_Number = 3;
        public const UInt32 CycloneInformation_Cyclone_Type_String = 4;

        public const UInt32 firmware_update_auto = 0;
        public const UInt32 firmware_update_force = 1;
        public const UInt32 firmware_update_never = 2;

        public const string DLL_FILENAME = "CycloneControlSDK.dll";

        /* The following imported methods have wrapper methods to properly handle the
           char* and char** C data types. 4 byte C# style bool is also converted into 
           a 1 byte C-style bool. Your application should call the wrapper method. */

        /* Private Methods */
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "version")]
        private static extern IntPtr unmanaged_version();
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "queryInformationOfAutodetectedCyclone")]
        private static extern IntPtr unmanaged_queryInformationOfAutodetectedCyclone(Int32 autodetectIndex, Int32 informationType);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "connectToCyclone")]
        private static extern UInt32 unmanaged_connectToCyclone(IntPtr nameIPOrPortIdentifier);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "setLocalMachineIpNumber")]
        private static extern void unmanaged_setLocalMachineIpNumber(IntPtr ipNumber);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "startDynamicDataProgram")]
        private static extern byte unmanaged_startDynamicDataProgram(UInt32 cycloneHandle, UInt32 targetAddress, UInt32 dataLength, IntPtr unmanagedCharArrayPtr);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "dynamicReadBytes")]
        private static extern byte unmanaged_dynamicReadBytes(UInt32 cycloneHandle, UInt32 targetAddress, UInt32 dataLength, IntPtr unmanagedCharArrayPtr);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getDescriptionOfErrorCode")]
        private static extern IntPtr unmanaged_getDescriptionOfErrorCode(UInt32 cycloneHandle, UInt32 errorCode);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getImageDescription")]
        private static extern IntPtr unmanaged_getImageDescription(UInt32 cycloneHandle, UInt32 imageId);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "compareImageInCycloneWithFile")]
        private static extern byte unmanaged_compareImageInCycloneWithFile(UInt32 cycloneHandle, IntPtr aFile, UInt32 imageId);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "addCycloneImage")]
        private static extern UInt32 unmanaged_addCycloneImage(UInt32 cycloneHandle, UInt32 selectedMediaType, byte replaceImageOfSameDescription, IntPtr aFile);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getPropertyValue")]
        private static extern IntPtr unmanaged_getPropertyValue(UInt32 cycloneHandle, UInt32 resourceOrImageID, IntPtr categoryName, IntPtr propertyName);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "setPropertyValue")]
        private static extern byte unmanaged_setPropertyValue(UInt32 cycloneHandle, UInt32 resourceOrImageId, IntPtr categoryName, IntPtr propertyName, IntPtr newValue);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getPropertyList")]
        private static extern IntPtr unmanaged_getPropertyList(UInt32 cycloneHandle, UInt32 resourceOrImageId, IntPtr categoryName);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getFirmwareVersion")]
        private static extern IntPtr unmanaged_getFirmwareVersion(UInt32 cycloneHandle);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "startImageExecution")]
        private static extern byte unmanaged_startImageExecution(UInt32 cycloneHandle, byte imageId);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "resetCyclone")]
        private static extern byte unmanaged_resetCyclone(UInt32 cycloneHandle, UInt32 resetDelayInMs);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "formatCycloneMemorySpace")]
        private static extern byte unmanaged_formatCycloneMemorySpace(UInt32 cycloneHandle, UInt32 selectedMediaType);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "eraseCycloneImage")]
        private static extern byte unmanaged_eraseCycloneImage(UInt32 cycloneHandle, UInt32 imageId);

        /* Public Methods */
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "enumerateAllPorts")]
        public static extern void enumerateAllPorts();
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "disconnectFromAllCyclones")]
        public static extern void disconnectFromAllCyclones();
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "queryNumberOfAutodetectedCyclones")]
        public static extern UInt32 queryNumberOfAutodetectedCyclones();
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "checkCycloneExecutionStatus")]
        public static extern UInt32 checkCycloneExecutionStatus(UInt32 cycloneHandle);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getNumberOfErrors")]
        public static extern UInt32 getNumberOfErrors(UInt32 cycloneHandle);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getErrorCode")]
        public static extern Int32 getErrorCode(UInt32 cycloneHandle, UInt32 errorNum);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "getLastErrorAddr")]
        public static extern UInt32 getLastErrorAddr(UInt32 cycloneHandle);
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "countCycloneImages")]
        public static extern UInt32 countCycloneImages(UInt32 cycloneHandle);
        
        // It is recommended to use the provided wrapper methods for executing special features that deal with unmanaged data types
        [DllImport(DLL_FILENAME, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl, EntryPoint = "cycloneSpecialFeatures")]
        public static extern byte unmanaged_cycloneSpecialFeatures(UInt32 featureNum, byte setFeature, UInt32 paramValue1, UInt32 paramValue2, UInt32 paramValue3, IntPtr paramReference1, IntPtr paramReference2);

        public static string version()
        {
            return (Marshal.PtrToStringAnsi(unmanaged_version()));
        }

        public static string queryInformationOfAutodetectedCyclone(Int32 autodetectIndex, Int32 informationType)
        {
            return (Marshal.PtrToStringAnsi(unmanaged_queryInformationOfAutodetectedCyclone(autodetectIndex, informationType)));
        }

        public static UInt32 connectToCyclone(string nameIPOrPortIdentifier)
        {
           IntPtr ptr = Marshal.StringToHGlobalAnsi(nameIPOrPortIdentifier);
           try
           {
                return (unmanaged_connectToCyclone(ptr));
           }
           finally
           {
                Marshal.FreeHGlobal(ptr);
           }
        }

        public static void setLocalMachineIpNumber(string ipNumber)
        {
            IntPtr ptr = Marshal.StringToHGlobalAnsi(ipNumber);
            try
            {
                unmanaged_setLocalMachineIpNumber(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static bool startDynamicDataProgram(UInt32 cycloneHandle, UInt32 targetAddress, UInt32 dataLength, Byte[] managedByteArray)
        {
            int size = managedByteArray.Length;
            IntPtr unmanagedByteArrayPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(managedByteArray, 0, unmanagedByteArrayPtr, managedByteArray.Length);
            try
            {
                byte unmanagedBool = unmanaged_startDynamicDataProgram(cycloneHandle, targetAddress, dataLength, unmanagedByteArrayPtr);
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedByteArrayPtr);
            }
        }

        public static Byte[] dynamicReadBytes(UInt32 cycloneHandle, UInt32 targetAddress, UInt32 dataLength)
        {
            //This function returns a managed byte array of size dataLength with data read from memory starting at targetAddress
            //If there is any error while reading memory, return a null.
            IntPtr unmanagedByteArrayPtr = Marshal.AllocHGlobal((int)dataLength);
            Byte[] managedByteArray = new Byte[dataLength];
            try
            {
                byte unmanagedBool = unmanaged_dynamicReadBytes(cycloneHandle, targetAddress, dataLength, unmanagedByteArrayPtr);
                if (unmanagedBool.Equals(0))
                {
                    return null;
                }
                else
                {
                    Marshal.Copy(unmanagedByteArrayPtr, managedByteArray, 0, (Int32)dataLength);
                    return managedByteArray;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedByteArrayPtr);
            }
        }

        public static string getDescriptionOfErrorCode(UInt32 cycloneHandle, UInt32 errorCode)
        {
                return (Marshal.PtrToStringAnsi(unmanaged_getDescriptionOfErrorCode(cycloneHandle, errorCode)));
        }

        public static string getImageDescription(UInt32 cycloneHandle, UInt32 imageId)
        {
            return (Marshal.PtrToStringAnsi(unmanaged_getImageDescription(cycloneHandle, imageId)));
        }

        public static bool compareImageInCycloneWithFile(UInt32 cycloneHandle, string aFile, UInt32 imageId)
        {
            IntPtr aFilePtr = Marshal.StringToHGlobalAnsi(aFile);
            try
            {
                byte unmanagedBool = Convert.ToByte(unmanaged_compareImageInCycloneWithFile(cycloneHandle, aFilePtr, imageId));
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(aFilePtr);
            }
        }

        public static UInt32 addCycloneImage(UInt32 cycloneHandle, UInt32 selectedMediaType, bool replaceImageOfSameDescription, string aFile)
        {
            IntPtr aFilePtr = Marshal.StringToHGlobalAnsi(aFile);
            try
            {
                byte unmanagedBool;
                if (!replaceImageOfSameDescription)
                {
                    unmanagedBool = 0;
                }
                else
                {
                    unmanagedBool = 1;
                }
                return (unmanaged_addCycloneImage(cycloneHandle, selectedMediaType, unmanagedBool, aFilePtr));
            }
            finally
            {
                Marshal.FreeHGlobal(aFilePtr);
            }
        }

        public static string getPropertyValue(UInt32 cycloneHandle, UInt32 resourceOrImageID, string categoryName, string propertyName)
        {
            IntPtr categoryNamePtr = Marshal.StringToHGlobalAnsi(categoryName);
            IntPtr propertyNamePtr = Marshal.StringToHGlobalAnsi(propertyName);
            try
            {
                return (Marshal.PtrToStringAnsi(unmanaged_getPropertyValue(cycloneHandle, resourceOrImageID, categoryNamePtr, propertyNamePtr)));
            }
            finally
            {
                Marshal.FreeHGlobal(categoryNamePtr);
                Marshal.FreeHGlobal(propertyNamePtr);
            }
        }

        public static bool setPropertyValue(UInt32 cycloneHandle, UInt32 resourceOrImageId, string categoryName, string propertyName, string newValue)
        {
            IntPtr categoryNamePtr = Marshal.StringToHGlobalAnsi(categoryName);
            IntPtr propertyNamePtr = Marshal.StringToHGlobalAnsi(propertyName);
            IntPtr newValuePtr = Marshal.StringToHGlobalAnsi(newValue);
            try
            {
                byte unmanagedBool = unmanaged_setPropertyValue(cycloneHandle, resourceOrImageId, categoryNamePtr, propertyNamePtr, newValuePtr);
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(categoryNamePtr);
                Marshal.FreeHGlobal(propertyNamePtr);
                Marshal.FreeHGlobal(newValuePtr);
            }
        }

        public static string getPropertyList(UInt32 cycloneHandle, UInt32 resourceOrImageId, string categoryName)
        {
            IntPtr categoryNamePtr = Marshal.StringToHGlobalAnsi(categoryName);
            try
            {
                return (Marshal.PtrToStringAnsi(unmanaged_getPropertyList(cycloneHandle, resourceOrImageId, categoryNamePtr)));
            }
            finally
            {
                Marshal.FreeHGlobal(categoryNamePtr);
            }
        }

        public static string getFirmwareVersion(UInt32 cycloneHandle)
        {
            return (Marshal.PtrToStringAnsi(unmanaged_getFirmwareVersion(cycloneHandle)));
        }

        public static bool startImageExecution(UInt32 cycloneHandle, byte imageId)
        {
            byte unmanagedBool = unmanaged_startImageExecution(cycloneHandle, imageId);
            if (unmanagedBool.Equals(0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool resetCyclone(UInt32 cycloneHandle, UInt32 resetDelayInMs)
        {
            byte unmanagedBool = unmanaged_resetCyclone(cycloneHandle, resetDelayInMs);
            if (unmanagedBool.Equals(0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool formatCycloneMemorySpace(UInt32 cycloneHandle, UInt32 selectedMediaType)
        {
            byte unmanagedBool = unmanaged_formatCycloneMemorySpace(cycloneHandle, selectedMediaType);
            if (unmanagedBool.Equals(0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool eraseCycloneImage(UInt32 cycloneHandle, UInt32 imageId)
        {
            byte unmanagedBool = unmanaged_eraseCycloneImage(cycloneHandle, imageId);
            if (unmanagedBool.Equals(0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool cycloneSpecialFeaturesGetImageDescriptionFromImageFile(string imageFileName, ref string resultString)
        {
            IntPtr tmpPtrToPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(IntPtr)));
            IntPtr filePtr = Marshal.StringToHGlobalAnsi(imageFileName);
            try
            {
                byte falseValue = 0;
                byte unmanagedBool = unmanaged_cycloneSpecialFeatures(CYCLONE_GET_IMAGE_DESCRIPTION_FROM_FILE, falseValue, 0, 0, 0, tmpPtrToPtr, filePtr);
                resultString = Marshal.PtrToStringAnsi((IntPtr)Marshal.PtrToStructure(tmpPtrToPtr, typeof(IntPtr)));
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tmpPtrToPtr);
                Marshal.FreeHGlobal(filePtr);
            }
        }

        public static bool cycloneSpecialFeaturesGetImageCRC32FromFile(string imageFileName, ref UInt32 numCRC32)
        {
            IntPtr tmpPtrToPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(UInt32)));
            IntPtr filePtr = Marshal.StringToHGlobalAnsi(imageFileName);
            try
            {
                byte falseValue = 0;
                byte unmanagedBool = unmanaged_cycloneSpecialFeatures(CYCLONE_GET_IMAGE_CRC32_FROM_FILE, falseValue, 0, 0, 0, tmpPtrToPtr, filePtr);
                numCRC32 = (UInt32) Marshal.ReadInt32(tmpPtrToPtr); //read the CRC32 value from the unmanaged pointer
                //stringCRC32 = numCRC32.ToString("X8"); //convert to hex string
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tmpPtrToPtr);
                Marshal.FreeHGlobal(filePtr);
            }
        }

        public static bool cycloneSpecialFeaturesGetImageScriptFileFromImageFile(string imageFileName, string scriptFile)
        {
            IntPtr scriptFilePtr = Marshal.StringToHGlobalAnsi(scriptFile);
            IntPtr imageFileNamePtr = Marshal.StringToHGlobalAnsi(imageFileName);
            try
            {
                byte falseValue = 0;
                byte unmanagedBool = unmanaged_cycloneSpecialFeatures(CYCLONE_GET_IMAGE_SCRIPT_FILE_FROM_FILE, falseValue, 0, 0, 0, scriptFilePtr, imageFileNamePtr);
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(scriptFilePtr);
                Marshal.FreeHGlobal(imageFileNamePtr);
            }
        }

        public static bool cycloneSpecialFeaturesTogglePowerNoDebug()
        {
            byte falseValue = 0;
            byte unmanagedBool = unmanaged_cycloneSpecialFeatures(CYCLONE_TOGGLE_POWER_NO_DEBUG, falseValue, 0, 0, 0, IntPtr.Zero, IntPtr.Zero);
            if (unmanagedBool.Equals(0))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static bool cycloneSpecialFeaturesSetActiveSecurityCode(UInt32 length, Byte[] securityBytesArray, string securityCodeType)
        {
            int size = securityBytesArray.Length;
            IntPtr unmanagedSecurityBytesArrayPtr = Marshal.AllocHGlobal(size);
            Marshal.Copy(securityBytesArray, 0, unmanagedSecurityBytesArrayPtr, securityBytesArray.Length);
            IntPtr securityCodeTypePtr = Marshal.StringToHGlobalAnsi(securityCodeType);
            try
            {
                byte trueValue = 1;
                byte unmanagedBool = unmanaged_cycloneSpecialFeatures(CYCLONE_SET_ACTIVE_SECURITY_CODE, trueValue, 0, length, 0, unmanagedSecurityBytesArrayPtr, securityCodeTypePtr);
                if (unmanagedBool.Equals(0))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(unmanagedSecurityBytesArrayPtr);
                Marshal.FreeHGlobal(securityCodeTypePtr);
            }
        }
    }
}