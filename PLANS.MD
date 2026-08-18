# Codex Execution Plans (ExecPlans)

This document describes the requirements for an execution plan ("ExecPlan"): a
design document that a coding agent can follow to deliver a working feature or
system change. Treat the reader as a complete beginner to this repository. They
have only the current working tree and the single ExecPlan file; they have no
memory of prior plans or external context.

## How to use ExecPlans and PLANS.md

When authoring an executable specification (ExecPlan), follow `PLANS.MD` to the
letter. If it is not in context, read the entire file. Research source material
thoroughly, start from the skeleton below, and expand it as you learn.

When implementing an ExecPlan, proceed to the next milestone without asking the
user for next steps. Keep all sections current. At every stopping point, add or
split progress entries to state the work done and the next work required. Resolve
ambiguities autonomously and commit frequently when commits are in scope.

When discussing an ExecPlan, record decisions in its log. ExecPlans are living
documents: it must always be possible to restart using only the ExecPlan and the
working tree. For challenging requirements or important unknowns, use milestones
for small prototypes that prove feasibility before the full implementation.

## Requirements

Every ExecPlan must be fully self-contained. It contains all knowledge and
instructions needed for a novice to succeed, and it remains self-contained after
each revision. It must enable a complete novice to implement the feature end to
end, produce demonstrably working behavior rather than merely code changes, and
define every non-ordinary term in plain language.

Begin with purpose and user value. Explain what someone can do after the change
that they could not do before, and how they can see it working. Then give the
exact edits, commands, and expected observations.

The implementing agent can list files, read files, search, run the project, and
run tests, but cannot infer unstated intent. Repeat every assumption you rely on.
Do not depend on external blogs or documents; embed the necessary knowledge in
the plan. If building on a checked-in plan, reference it explicitly. If it is not
checked in, incorporate all relevant context.

## Formatting

In chat, write an ExecPlan in one fenced code block labeled `md`, enclosed by one
opening and one closing triple-backtick line. Do not nest fences. Present commands,
transcripts, diffs, and code as indented text within that block.

When the content of a Markdown file is only the ExecPlan, omit the outer fences.
Use plain prose and prefer sentences over lists. Checklists are allowed only in
the mandatory `Progress` section. Use valid Markdown headings and two blank lines
after each heading.

## Guidance

Anchor the plan in observable outcomes. State exact commands, the working
directory, and useful expected output. Phrase acceptance as behavior a person can
verify, such as an HTTP endpoint returning a stated status and response body. For
internal work, explain how tests or a small end-to-end scenario prove its impact.

Name all repository-relative paths, functions, modules, and new files precisely.
If multiple areas are touched, orient a novice by explaining how they fit
together. State environment assumptions and reasonable alternatives.

Make steps safe to repeat. Explain how to retry a partially failed operation and
provide backups, recovery, or safe fallbacks before destructive work. Prefer
additive, testable changes. Validation is mandatory: include tests, how to start
the system if needed, observable behavior beyond successful compilation, and
concise evidence such as expected logs or focused diffs.

Milestones tell the story of the work. Each milestone must be independently
verifiable and incrementally deliver the overall outcome. Introduce each one with
its scope, new behavior, commands, and expected acceptance result. Prototypes and
parallel implementations are allowed when they reduce risk; label them clearly,
explain how to validate them, and state promotion or removal criteria.

## Living plans and design decisions

Every ExecPlan must contain and maintain all of these sections:

- `Progress`: a timestamped checkbox list of granular work. It must reflect the
  actual state at every stopping point, including partial completion and the
  remaining work.
- `Surprises & Discoveries`: unexpected behavior, bugs, tradeoffs, or insights,
  supported by concise evidence.
- `Decision Log`: every material decision, its rationale, and date/author.
- `Outcomes & Retrospective`: results, gaps, and lessons at major milestones or
  completion, compared with the original purpose.

When revising a plan, update all affected sections and add a note at its end
describing what changed and why.

## Skeleton of a good ExecPlan

    # <Short, action-oriented description>

    This ExecPlan is a living document. Keep `Progress`, `Surprises & Discoveries`,
    `Decision Log`, and `Outcomes & Retrospective` up to date as work proceeds.
    Follow `PLANS.MD` from the repository root.

    ## Purpose / Big Picture

    Explain the user-visible capability this work enables and how someone can
    observe it working.

    ## Progress

    - [x] (2026-08-18 10:00Z) Example completed step.
    - [ ] Example incomplete step.
    - [ ] Example partial step (completed: X; remaining: Y).

    ## Surprises & Discoveries

    - Observation: Describe the unexpected result.
      Evidence: Include a concise command, test output, or other proof.

    ## Decision Log

    - Decision: State the chosen approach.
      Rationale: Explain why it was chosen.
      Date/Author: 2026-08-18 / <author>.

    ## Outcomes & Retrospective

    Compare the completed behavior with the original purpose. Record remaining
    work and lessons learned.

    ## Context and Orientation

    Explain relevant repository state for a reader who knows nothing about it.
    Name key paths and define non-obvious terms. Do not rely on prior plans.

    ## Plan of Work

    Describe edits in prose. For every edit, name the file, location, exact
    change, and why it is necessary.

    ## Concrete Steps

    Give exact commands and working directories, with short expected output where
    it helps the reader recognize success.

    ## Validation and Acceptance

    Describe tests and an observable scenario. State inputs, expected results,
    and how to interpret failures.

    ## Idempotence and Recovery

    Explain what is safe to repeat, how to retry failures, and safe rollback or
    cleanup for risky steps.

    ## Artifacts and Notes

    Include concise transcripts, diffs, or snippets that prove important results.

    ## Interfaces and Dependencies

    Name required libraries, modules, services, public types, and function
    signatures. State why each is used and the stable path where it must exist.
