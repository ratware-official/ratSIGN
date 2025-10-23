## ratSIGN

**ratSIGN** is a command-line utility for **secure file signing and verification** based on the  
[ratCORE.Signing](https://github.com/ratware-official/ratCORE.Signing) library (ECDSA P-256 + SHA-256).  
It enables developers and release pipelines to generate cryptographically strong digital signatures,  
ensuring the authenticity and integrity of published files.

---

### 🚀 Features

- **Sign files** using encrypted private key files (`.sec.json`) and a secret password.  
- **Verify files** using their detached signature (`.ratsig`) and either:
  - a **public key** (Base64 uncompressed EC point), or  
  - a **KeyId** (`Base64(SHA256(pub))`) as trust anchor.  
- **Generate keys** securely via PBKDF2-SHA256 + AES-256-GCM encryption.  
- **Cross-platform:** works on **Windows**, **Linux**, and **macOS**.  
- **No dependencies** beyond .NET 8.

---

### ⚙️ Build & Publish

Framework-dependent (requires .NET runtime to be installed):
- Windows: `dotnet publish -c Release -r win-x64 --self-contained false`
- Linux:   `dotnet publish -c Release -r linux-x64 --self-contained false`
- macOS:   `dotnet publish -c Release -r osx-arm64 --self-contained false`

Self-contained (includes .NET runtime in the binary):
- Windows: `dotnet publish -c Release -r win-x64 --self-contained true`
- Linux:   `dotnet publish -c Release -r linux-x64 --self-contained true`
- macOS:   `dotnet publish -c Release -r osx-arm64 --self-contained true`

Single-file (bundles all dependencies into one executable; avoid trimming because of reflection):
- Windows: `dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false`
- Linux:   `dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false`
- macOS:   `dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true -p:PublishTrimmed=false`

> **Note:**  
> - For *framework-dependent* builds, distribute the **entire `publish/` folder** (including `ratCORE.Signing.dll`).  
> - For *self-contained single-file* builds, distribute **only the generated executable**.

---

### 🧩 Commands Overview

| Command | Description |
|----------|-------------|
| `keygen` | Generates a new ECDSA-P256 key pair and writes an encrypted key file (`.sec.json`). |
| `sign` | Signs a file using an encrypted key file and password. Produces `.ratsig`. |
| `verify` | Verifies a file using its `.ratsig` and a trusted public key or KeyId. |
| `keyid` | Computes a KeyId (`Base64(SHA256(pub))`) for a given public key. |
| `version` | Displays version information and license details. |

---

### 🔐 Usage

```bash
ratsign [command] [options]
```

#### Generate a key
```bash
ratsign keygen --out . --iterations 300000 --name release-key
```
- Creates a password-protected key file: `ratsign_<id>.sec.json`.

#### Sign a file
```bash
ratsign sign --file ./payload.bin --key ./release-key.sec.json --comment "release:1"
```
- Produces `payload.bin.ratsig`, containing the signature and metadata.

#### Verify a file (with public key)
```bash
ratsign verify --file ./payload.bin --sig ./payload.bin.ratsig --pub BM5X...LFfU=
```

#### Verify a file (with KeyId)
```bash
ratsign verify --file ./payload.bin --sig ./payload.bin.ratsig --keyid 8/zk...8LEs=
```

#### Compute KeyId from a public key
```bash
ratsign keyid --pub BM5X...LFfU=
```

#### Show version information
```bash
ratsign version
```

---

### ⚙️ Options Summary

#### `keygen`
| Option | Description |
|---------|-------------|
| `--out <dir>` | Output directory for the key file (default: current). |
| `--iterations <N>` | PBKDF2 iterations (default: 300000). |
| `--name <base>` | Optional base name for the key file (default: auto from KeyId). |

#### `sign`
| Option | Description |
|---------|-------------|
| `--file <path>` | File to sign. |
| `--key <key.sec.json>` | Encrypted key file. |
| `--out <sig.ratsig>` | Output signature file (default: `<file>.ratsig`). |
| `--comment "<text>"` | Optional comment included in the signature. |

#### `verify`
| Option | Description |
|---------|-------------|
| `--file <path>` | File to verify. |
| `--sig <sig.ratsig>` | Detached signature file. |
| `--pub <base64>` | Trusted public key (uncompressed 0x04||X||Y, Base64). |
| `--keyid <base64>` | Trusted KeyId (`Base64(SHA256(pub))`). |

#### `keyid`
| Option | Description |
|---------|-------------|
| `--pub <base64>` | Public key to calculate KeyId from. |

---

### 🧠 Notes

- **Algorithm:** ECDSA P-256 with SHA-256  
- **Public key format:** `0x04 || X(32) || Y(32)` (uncompressed EC point, Base64-encoded)  
- **KeyId:** `Base64(SHA256(pub))` — recommended for trust verification  
- **Encryption:** AES-256-GCM with PBKDF2-SHA256 derived keys  
- **Signatures:** DER-encoded, stored as Base64 in `.ratsig` JSON

---

### 🧱 Exit Codes

| Code | Meaning |
|------|----------|
| `0` | Success (valid signature / command completed) |
| `1` | Verification failed (invalid signature or untrusted key) |
| `2` | General error (invalid arguments, I/O, crypto failure) |

---

### 🧩 Example Workflow

```bash
# Generate key
ratsign keygen --out ./keys --name build2025

# Sign file
ratsign sign --file ./release.zip --key ./keys/build2025.sec.json --comment "release build 2025"

# Verify (trusted keyid)
ratsign verify --file ./release.zip --sig ./release.zip.ratsig --keyid 8/zk...8LEs=
```

---

### 📦 Requirements

- .NET 8 Runtime or SDK  
- [ratCORE.Signing](https://github.com/ratware-official/ratCORE.Signing) library  
- Supported OS: **Windows**, **Linux**, **macOS**  

---

### 🧩 About

**ratSIGN** is part of the **ratCORE** framework — a suite of lightweight, secure, and reusable tools  
for developers and system maintainers.  
It provides an easy-to-use interface for signing and verifying digital content.

---

**License:** Creative Commons Attribution 4.0 International (CC BY 4.0)  
**Copyright © 2025 ratware**
