#!/usr/bin/env bash

# Build the test WebAssembly modules for the unit tests.

pushd tests/implementations/csharp
dotnet build -c Release
popd

pushd tests/implementations/javascript
yarn
yarn build
popd

cp tests/implementations/csharp/bin/Release/net10.0/wasi-wasm/publish/tests.wasm tests/Wasmtime.Tests/wasm/csharp.wasm
cp tests/implementations/javascript/tests.wasm tests/Wasmtime.Tests/wasm/javascript.wasm