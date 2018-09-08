# MySecureNotes
Basic applied cryptography to keep private records

Security
- AES-256 encryption
- RFC2898 (PBKDF2) key derivation
- Unique IV salt for each record
- Hashed record is embedded at random position in a buffer of random noise before encryption

Designed to be used with version control systems
- Encrypted records are stored as separate strings
- If a record is changed, adjacent records are not altered

