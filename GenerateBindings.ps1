$WasmtimeGeneratedSource = './src/Interop/Native.cs'

ClangSharpPInvokeGenerator `
    -c compatible-codegen exclude-funcs-with-body `
    --file .\c-api\windows\wasmtime\wasmtime.h `
    --traverse .\c-api\windows\wasmtime\wasm.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\wasi.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\module.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\instance.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\store.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\error.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\wasip2.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\extern.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\linker.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\sharedmemory.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\component\instance.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\component\val.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\component\linker.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\component\func.h `
    --traverse .\c-api\windows\wasmtime\wasmtime\component\component.h `
    --include-directory .\c-api\windows\wasmtime `
    --include-directory .\c-api\windows\wasi `
    -n Wasmtime.Interop `
    --methodClassName WasmtimeSource `
    --libraryPath wasmtime `
    -o $WasmtimeGeneratedSource

(Get-Content $WasmtimeGeneratedSource) `
    -replace "public", "internal" `
    -replace "const UIntPtr", "const uint" `
    -replace "sbyte[*]", "byte*" `
    | Out-File $WasmtimeGeneratedSource

    
#    -replace "(?<name>[a-zA-Z0-9_-]+)\* \@out", 'out ${name} @out' `