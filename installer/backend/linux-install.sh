#!/usr/bin/env bash
set -euo pipefail

if [[ "${EUID}" -ne 0 ]]; then
    echo "Futtasd a szkriptet sudo-val (root jogosultsággal)."
    exit 1
fi

if ! command -v apt >/dev/null 2>&1; then
    echo "Ez a szkript csak Debian/Ubuntu alapú rendszereken futtatható."
    exit 1
fi

REPO_URL="https://github.com/sztanyikz2/vizsgaprojekt.git"
TARGET_DIR="${1:-vizsgaprojekt}"

echo "Csomaglista frissítése..."
apt update

echo "Szükséges függőségek telepítése..."
apt install -y git curl ca-certificates

if command -v dotnet >/dev/null 2>&1; then
    DOTNET_VERSION="$(dotnet --version)"
    DOTNET_MAJOR="${DOTNET_VERSION%%.*}"
    if [[ "${DOTNET_MAJOR}" -lt 8 ]]; then
        echo "A telepített .NET SDK verzió túl régi: ${DOTNET_VERSION}"
        echo "Ehhez a projekthez legalább .NET 8 SDK szükséges."
        exit 1
    fi
    echo ".NET telepítve: ${DOTNET_VERSION}"
else
    echo "A .NET nincs telepítve."
    echo "Telepítsd a .NET SDK-t, majd futtasd újra a szkriptet:"
    echo "https://learn.microsoft.com/dotnet/core/install/linux"
    exit 1
fi

if [[ -d "${TARGET_DIR}" ]]; then
    echo "A célmappa már létezik: ${TARGET_DIR}"
    exit 1
fi

echo "Repository klónozása ide: ${TARGET_DIR}"
git clone "${REPO_URL}" "${TARGET_DIR}"

echo "Kész. A projekt klónozva lett: ${TARGET_DIR}"

echo "Szeretnéd-e futtatni a projektet most? (y/n)"
read -r RUN_NOW

if [[ "${RUN_NOW}" == "y" ]]; then
    echo "Projekt futtatása..."
    cd "${TARGET_DIR}"
    dotnet run --project backend_controller/vizsgaController.csproj
else
    echo "A projekt később a következő paranccsal indítható el:"
    echo "cd ${TARGET_DIR} && dotnet run --project backend_controller/vizsgaController.csproj"
fi