# Avatar User dengan Inisial - Testing Guide

## Perubahan yang Dilakukan

### 1. **AuthController.cs**

Menambahkan email ke claims saat login:

```csharp
new Claim(ClaimTypes.Email, user.Email ?? ""),
new Claim("email", user.Email ?? "")
```

### 2. **\_LayoutDashboard.cshtml**

#### Avatar dengan Inisial

- Foto user diganti dengan circle avatar berisi inisial nama
- Background warna primary (biru)
- Font putih, bold, uppercase

#### Data User Dinamis

- **Nama**: Dari `User.Identity.Name` (username)
- **Email**: Dari claims `ClaimTypes.Email` atau `"email"`
- **Inisial**: Generated dari helper function `GetUserInitials()`

#### Badge Role

- `SUPER_ADMIN` → Badge hijau "SUPER ADMIN"
- `ADMIN` → Badge biru "ADMIN"
- User biasa → Badge info "USER"

### 3. **Helper Function**

```csharp
GetUserInitials(string name)
```

- Single word (contoh: "Admin") → "AD" (2 huruf pertama)
- Multiple words (contoh: "Super Admin") → "SA" (first char dari 2 kata pertama)
- Empty/null → "U"

## Contoh Output

### User: "Super Admin"

- Inisial: **SA**
- Display: Circle avatar biru dengan "SA" putih
- Badge: "SUPER ADMIN" (hijau)
- Email: email dari database

### User: "adminuser"

- Inisial: **AD**
- Display: Circle avatar biru dengan "AD" putih
- Badge: "ADMIN" (biru) atau "USER" (info)
- Email: email dari database

### User: "John Doe"

- Inisial: **JD**
- Display: Circle avatar biru dengan "JD" putih
- Badge: Sesuai role
- Email: email dari database

## Testing Steps

1. **Login dengan user berbeda**
   - Super Admin
   - Admin
   - User biasa

2. **Cek tampilan avatar di header**
   - Harus muncul circle dengan inisial
   - Warna background biru
   - Text putih dan uppercase

3. **Klik dropdown profile**
   - Nama user harus sesuai dengan yang login
   - Email harus sesuai dengan data di database
   - Badge role harus sesuai (SUPER ADMIN / ADMIN / USER)
   - Avatar inisial sama dengan di header

4. **Test dengan nama berbeda**
   - Single word: "Admin" → "AD"
   - Two words: "John Doe" → "JD"
   - Three words: "Super Power Admin" → "SP"

## CSS Classes

Avatar menggunakan class:

- `.avatar-text` - Base class untuk avatar circular
- `.bg-primary` - Background biru
- `.text-white` - Text putih
- `.user-avtar` - Specific untuk user avatar

Size:

- Header: 40x40px, font 14px
- Dropdown header: 48x48px, font 16px

## Fallback

Jika data tidak tersedia:

- Username → "User"
- Email → "user@example.com"
- Inisial → "U"
