$WasmtimeGeneratedSource = './src/Wasmtime/Interop/Native.cs'
$WasmtimeSdk = './src/Wasmtime/obj/wasmtime-dev-x86_64-windows-c-api/include'
$WasiPath = Join-Path (Join-Path ( [Environment]::GetFolderPath("UserProfile") ) ".wasi-sdk") "wasi-sdk-24"

ClangSharpPInvokeGenerator `
    -c compatible-codegen exclude-funcs-with-body `
    --file $WasmtimeSdk/wasmtime.h `
    --traverse $WasmtimeSdk/wasm.h `
    --traverse $WasmtimeSdk/wasmtime/wasi.h `
    --traverse $WasmtimeSdk/wasmtime/module.h `
    --traverse $WasmtimeSdk/wasmtime/instance.h `
    --traverse $WasmtimeSdk/wasmtime/store.h `
    --traverse $WasmtimeSdk/wasmtime/error.h `
    --traverse $WasmtimeSdk/wasmtime/wasip2.h `
    --traverse $WasmtimeSdk/wasmtime/extern.h `
    --traverse $WasmtimeSdk/wasmtime/linker.h `
    --traverse $WasmtimeSdk/wasmtime/sharedmemory.h `
    --traverse $WasmtimeSdk/wasmtime/component/instance.h `
    --traverse $WasmtimeSdk/wasmtime/component/val.h `
    --traverse $WasmtimeSdk/wasmtime/component/linker.h `
    --traverse $WasmtimeSdk/wasmtime/component/func.h `
    --traverse $WasmtimeSdk/wasmtime/component/component.h `
    --include-directory $WasiPath `
    --include-directory $WasmtimeSdk `
    -n Wasmtime.Interop `
    --methodClassName WasmtimeSource `
    --libraryPath wasmtime `
    -o $WasmtimeGeneratedSource

(Get-Content $WasmtimeGeneratedSource) `
    -replace "public", "internal" `
    -replace "const UIntPtr", "const uint" `
    -replace "sbyte[*]", "byte*" `
    | Out-File $WasmtimeGeneratedSource