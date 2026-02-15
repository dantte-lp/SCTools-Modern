# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 2.x     | Yes       |
| < 2.0   | No        |

## Reporting a Vulnerability

If you discover a security vulnerability, please report it responsibly:

1. **Do NOT** open a public GitHub issue
2. Send a private report via [GitHub Security Advisories](https://github.com/h0useRus/StarCitizen/security/advisories/new)
3. Include: description of the vulnerability, steps to reproduce, potential impact

You should receive an acknowledgment within 48 hours. We aim to release a fix within 7 days for critical vulnerabilities.

## Security Practices

- No secrets or tokens are stored in plaintext (Windows Credential Manager only)
- TLS uses system defaults (no protocol version hardcoding)
- All file paths are validated against directory traversal attacks
- URLs are validated (scheme allowlist) before launching external processes
- Content-Disposition headers are validated before file extraction
- NuGet dependency audit is enforced in CI (NU1901-NU1904 as errors)
- SHA-256 for file integrity verification (no MD5)
- All warnings treated as errors with maximum analyzer level
