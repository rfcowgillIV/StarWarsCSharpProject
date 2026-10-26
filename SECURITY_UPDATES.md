# Security & Bug-Fix Update — October 2026

Everything changed in the `security-updates-2026` branch, and why.

## 1. Vulnerable package: Newtonsoft.Json 6.0.7 → 13.0.3 (the likely GitHub alert)

Versions before 13.0.1 have **CVE-2024-21907**: a JSON document with very deep nesting can crash
the program (denial of service). Updated in
`SharpTrooper-master/SharpTrooper-master/packages.config` and the reference in `SharpTrooper.csproj`.
The JSON reader also caps nesting depth at 64 for extra safety.

**Bug fixed at the same time:** the reference's `HintPath` pointed to `..\packages\…`, but NuGet
restores packages next to the `.sln` file, two folders up. It's now `..\..\packages\…`, so Visual
Studio finds the library after restoring.

## 2. The API it talked to no longer exists

`http://swapi.co/api` shut down years ago, so the program crashed on the very first question.

- The default is now **`https://swapi.dev/api`**, the community-run replacement with the same data.
- If that site is down, point the program at another mirror without changing code:

  ```bat
  set SWAPI_BASE_URL=https://swapi.py4e.com/api
  ```

- It uses **HTTPS** and explicitly enables **TLS 1.2**, which older .NET Framework apps don't
  always turn on and most HTTPS sites require.

## 3. Framework versions

| Project | Was | Now | Why |
|---|---|---|---|
| StarWarsAPI | .NET Framework 4.6.1 | 4.8 | 4.6.1 has been out of support since April 2022 |
| SharpTrooper | .NET Framework 4.0 | 4.8 | 4.0 has been out of support since 2016 and can't use TLS 1.2 |

.NET Framework 4.8 ships with Windows 10/11, so there's usually nothing extra to install. If
Visual Studio asks for the "4.8 targeting pack," install it from the Visual Studio Installer.

## 4. Input safety (URL injection)

Whatever you typed as a film or character number was glued straight onto the web address. Typing
something like `../../somewhere?x=` changed which address the program called. Now:

- `Program.cs` only accepts whole numbers in range (films 1–6, characters 1–83).
- `SharpTrooperCore` URL-encodes every id and query value as a second line of defense.

## 5. Bugs fixed

| Bug | Fix |
|---|---|
| Typing a letter or out-of-range number crashed the program | Explains the problem and asks again |
| The prompt said films #1–#7, but the API only has 6 films | Says #1–#6 |
| Character #17 doesn't exist in the API (a known gap) and crashed the program | "Nothing found, pick another number" |
| No internet or API down crashed with an unhandled exception | Clear message and a tip about `SWAPI_BASE_URL` |
| Reading the Jedi Archives before saving anything crashed | Returns an empty string |
| Network responses weren't disposed when errors happened | `using` blocks release connections |
| `GetSingleByUrl<T>(object p)` was a leftover stub that only threw `NotImplementedException` | Removed |
| Query-string parsing broke on values containing `=` | Splits on the first `=` only |
| `UserDataUtility` didn't implement its own `IUserDataUtility` interface, and the file path was typed in two places | Implements the interface; one `ArchivePath` constant |

## 6. Removed dead files

- `StarWarsAPI/UserDataUtility.cs`: every line was commented out.
- `StarWarsAPI/FileIO.cs`: not part of the build, and it referenced an `IFileUtility` interface
  that doesn't exist.

## 7. How to verify

1. Open `StarWarsAPI.sln` in Visual Studio 2019 or newer (Community is free).
2. Right-click the solution → **Restore NuGet Packages**. Newtonsoft.Json 13.0.3 downloads.
3. Press **Start**. Try a letter, `0`, `17`, then valid numbers. The program should never crash.
4. Optional: **Tools → NuGet Package Manager → Manage Packages for Solution → Updates** should show
   no security warnings.

This environment had no .NET compiler, so the code couldn't be compiled before committing. Steps
1–3 above are the first real build, and any compiler error will point to the exact line.
