# Java source references

The authoritative server remains an independent .NET implementation. The Java
projects listed in `java-reference.lock.json` are read-only research sources for
Interlude behavior, formulas, content semantics, and original-client protocol
meaning.

Retrieve the primary L2J Mobius CT_0 Interlude source at its pinned revision:

```bash
./scripts/reference-java.sh setup mobius
./scripts/reference-java.sh verify mobius
```

When Mobius behavior is ambiguous or appears customized, retrieve the pinned
aCis revision for a secondary comparison:

```bash
./scripts/reference-java.sh setup acis
./scripts/reference-java.sh verify acis
```

`all` may be used in place of a reference name. Retrieved repositories are
ignored by Git. The helper will not replace, update, build, configure, or run an
existing clone; a mismatched or modified clone fails verification so local work
cannot be overwritten accidentally.

## Extraction rules

- Treat Mobius as the primary research source and aCis only as a cross-check.
- Exclude Mobius custom systems, convenience features, and non-retail settings
  from compatibility requirements.
- Record the reference name, commit, source paths, inputs, outputs, edge cases,
  and intentional deviations in each behavior note or imported record.
- Specify observable behavior before writing the .NET implementation.
- Do not copy Java classes, inheritance structures, packet-handler architecture,
  persistence code, or the original client protocol into production code.
- Do not commit Java sources, original game assets, or Java-reference generated
  output to this repository. ADR-0011 permits only its reviewed derived browser
  assets under the repository `assets/` directory; their private source package
  remains ignored.

The helper has no build or run operation by design. Java, Ant, and MySQL are not
project prerequisites, and the Java servers are not part of local development or
CI.
