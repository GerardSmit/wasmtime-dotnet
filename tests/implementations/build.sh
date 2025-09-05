#!/usr/bin/env bash

# Arguments:
#   implementation: The implementation to build. One of "csharp", "c", or "js".
#                   If not provided, all implementations will be built.

implementation=$1

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
pushd "$SCRIPT_DIR"

# Variables for determining platform
if [[ "$OS" == "Windows_NT" ]]; then
    PLATFORM="win"
    EXT=".exe"
elif [[ "$(uname)" == "Darwin" ]]; then
    PLATFORM="osx"
    EXT=""
else
    PLATFORM="linux"
    EXT=""
fi

ARCH=$(uname -m)
if [[ "$ARCH" == "x86_64" ]]; then
    ARCH="x64"
elif [[ "$ARCH" == "aarch64" || "$ARCH" == "arm64" ]]; then
    ARCH="arm64"
else
    # Default to x64 if architecture is not recognized
    ARCH="x64"
fi

if [[ -z "$implementation" || "$implementation" == "csharp" ]]; then
    rm -rf csharp/wit
    cp -r wit csharp

    pushd csharp
    dotnet build -c Release
    popd
    
    cp csharp/bin/Release/net10.0/wasi-wasm/publish/tests.wasm ../Wasmtime.Tests/wasm/csharp.wasm
fi

if [[ -z "$implementation" || "$implementation" == "c" ]]; then
    rm -rf c/wit
    cp -r wit c

    pushd c

    # Find the latest version of wit-bindgen
    WIT_BINDGEN_BASE_PATH=~/.nuget/packages/bytecodealliance.componentize.dotnet.witbindgen

    if [[ -d "$WIT_BINDGEN_BASE_PATH" ]]; then
        # Get the latest version by sorting versions and taking the last one
        LATEST_VERSION=$(ls "$WIT_BINDGEN_BASE_PATH" | sort -V | tail -n 1)
        WIT_BINDGEN_PATH="$WIT_BINDGEN_BASE_PATH/$LATEST_VERSION/tools/${PLATFORM}-${ARCH}/wit-bindgen${EXT}"
    else
        echo "Error: wit-bindgen package not found at $WIT_BINDGEN_BASE_PATH"
        exit 1
    fi

    $WIT_BINDGEN_PATH c wit/component.wit --world test

    ~/.wasi-sdk/wasi-sdk-24.0/bin/wasm32-wasip2-clang \
        -o c.wasm \
        -mexec-model=reactor \
        component.c \
        test.c \
        test_component_type.o

    popd

    cp c/c.wasm ../Wasmtime.Tests/wasm/c.wasm
fi

if [[ -z "$implementation" || "$implementation" == "javascript" || "$implementation" == "js" ]]; then
    rm -rf javascript/wit
    cp -r wit javascript

    pushd javascript
    yarn
    yarn build
    popd

    
    cp javascript/tests.wasm ../Wasmtime.Tests/wasm/javascript.wasm
fi

popd
