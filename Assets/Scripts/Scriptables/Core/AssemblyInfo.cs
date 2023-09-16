using System.Runtime.CompilerServices;

// Expose internal members to editor assembly for inspectors, other editor windows or functions
[assembly: InternalsVisibleTo("ScriptableAssembly.Editor")]
[assembly: InternalsVisibleTo("ScriptableAssembly.Data")]