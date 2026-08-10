#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
manifest="$repo_root/reference/java-reference.lock.json"

usage() {
  echo "Usage: $0 <setup|verify> [mobius|acis|all]" >&2
}

load_reference() {
  local name="$1"
  local result

  result="$(node -e '
    const fs = require("node:fs")
    const [manifestPath, name] = process.argv.slice(1)
    const manifest = JSON.parse(fs.readFileSync(manifestPath, "utf8"))
    const reference = manifest.references[name]
    if (!reference) process.exit(2)
    process.stdout.write([
      reference.repository,
      reference.commit,
      reference.target,
      reference.sparsePaths.join(",")
    ].join("\t"))
  ' "$manifest" "$name")" || {
    echo "Unknown or invalid Java reference: $name" >&2
    exit 2
  }

  IFS=$'\t' read -r reference_repository reference_commit reference_target reference_sparse_paths <<<"$result"
  reference_directory="$repo_root/$reference_target"
}

verify_one() {
  local name="$1"
  load_reference "$name"

  if [[ ! -d "$reference_directory/.git" ]]; then
    echo "$name: missing $reference_target (run setup first)" >&2
    return 1
  fi

  local actual_repository actual_commit changes
  actual_repository="$(git -C "$reference_directory" remote get-url origin)"
  actual_commit="$(git -C "$reference_directory" rev-parse HEAD)"
  changes="$(git -C "$reference_directory" status --porcelain)"

  if [[ "$actual_repository" != "$reference_repository" ]]; then
    echo "$name: origin is $actual_repository, expected $reference_repository" >&2
    return 1
  fi
  if [[ "$actual_commit" != "$reference_commit" ]]; then
    echo "$name: revision is $actual_commit, expected $reference_commit" >&2
    return 1
  fi
  if [[ -n "$changes" ]]; then
    echo "$name: reference clone has local changes" >&2
    return 1
  fi

  local sparse_path
  IFS=',' read -ra sparse_paths <<<"$reference_sparse_paths"
  for sparse_path in "${sparse_paths[@]}"; do
    if [[ ! -e "$reference_directory/$sparse_path" ]]; then
      echo "$name: sparse path is missing: $sparse_path" >&2
      return 1
    fi
  done

  echo "$name: verified $reference_commit"
}

setup_one() {
  local name="$1"
  load_reference "$name"

  if [[ -e "$reference_directory" ]]; then
    verify_one "$name"
    return
  fi

  local parent_directory temporary_directory
  parent_directory="$(dirname "$reference_directory")"
  mkdir -p "$parent_directory"
  temporary_directory="$(mktemp -d "$parent_directory/.${name}.tmp.XXXXXX")"

  cleanup() {
    if [[ -n "${temporary_directory:-}" && "$temporary_directory" == "$parent_directory/.${name}.tmp."* ]]; then
      rm -rf -- "$temporary_directory"
    fi
  }
  trap cleanup EXIT INT TERM

  git -C "$temporary_directory" init --quiet
  git -C "$temporary_directory" remote add origin "$reference_repository"
  git -C "$temporary_directory" sparse-checkout init --cone

  local sparse_path
  IFS=',' read -ra sparse_paths <<<"$reference_sparse_paths"
  git -C "$temporary_directory" sparse-checkout set "${sparse_paths[@]}"
  git -C "$temporary_directory" fetch --quiet --depth 1 --filter=blob:none origin "$reference_commit"
  git -C "$temporary_directory" checkout --quiet --detach FETCH_HEAD
  mv "$temporary_directory" "$reference_directory"
  trap - EXIT INT TERM

  verify_one "$name"
}

run_for_selection() {
  local operation="$1"
  local selection="$2"

  if [[ "$selection" == "all" ]]; then
    "$operation" mobius
    "$operation" acis
  else
    "$operation" "$selection"
  fi
}

if [[ $# -lt 1 || $# -gt 2 ]]; then
  usage
  exit 2
fi

command="$1"
selection="${2:-mobius}"
case "$selection" in
  mobius|acis|all) ;;
  *) usage; exit 2 ;;
esac

case "$command" in
  setup) run_for_selection setup_one "$selection" ;;
  verify) run_for_selection verify_one "$selection" ;;
  *) usage; exit 2 ;;
esac
