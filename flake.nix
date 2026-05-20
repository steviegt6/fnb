# Use the following command to update the ./nix/deps.json file:
#   Bash:    nix build .#default.fetch-deps && ./result
#   Nushell: nix build .#default.fetch-deps; ./result
# Then overwrite ./nix/deps.json with the file whose path is last printed.
{
  description = "A library and command-line tool for manipulating XNB and .tmod archive files.";

  inputs.self.submodules = true;

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    flake-utils.url = "github:numtide/flake-utils";
  };

  outputs = { self, nixpkgs, flake-utils, ... }: flake-utils.lib.eachDefaultSystem (system: let
    pkgs = import nixpkgs { inherit system; };
    dotnet-sdk = (pkgs.dotnetCorePackages.combinePackages [
      pkgs.dotnet-sdk_8
      pkgs.dotnet-sdk_9
    ]);
    dotnet-runtime = pkgs.dotnet-runtime_9;
  in {
    packages = rec {
      default = cli;
      cli = pkgs.buildDotnetModule {
        inherit dotnet-sdk dotnet-runtime;
        pname = "Tomat.FNB.CLI";
        version = "0.1.0";
        src = ./.;
        nugetDeps = ./nix/deps.json;
        projectFile = "./src/Tomat.FNB.CLI/Tomat.FNB.CLI.csproj";
      };
    };
    # Development shells provide the tooling necessary for development.
    devShells = {
      default = pkgs.mkShell {
        buildInputs = [
          dotnet-sdk
          pkgs.git
          pkgs.netcoredbg
        ];
      };
    };
  });
}
