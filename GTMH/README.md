All GTMH packages are signed with a code signing certificate.
During development, we use a self-signed certificate which may
show security warnings. This is expected and safe for development use.

To verify package signatures:
dotnet nuget verify GTMH.*.nupkg --all

