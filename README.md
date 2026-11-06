# VProPrograms

Cognex VisionPro 관련 프로그램 모음 (C# / .NET Framework 4.7.2 / WinForms, VisionPro 59).

## 프로젝트

| 폴더 | 내용 |
|---|---|
| `QB_Scripts/` | QuickBuild ToolBlock/ToolGroup 스크립트(.cs) 모음 |
| `VproBlack/` | VisionPro 초기 시험 프로젝트 |
| `VproManageBlack01/` | QuickBuild VPP 로드·검사 프로그램 1차 |
| `VproManageBlack02/` | QuickBuild VPP 로드·검사 프로그램 2차 (단계별 개선, 브랜치 `fix/stage1` 기준) |
| `VProManager01/` | VPP 검사 관리 프로그램 (FHD 고정 배치) |
| `VProManager02/` | VPP 검사 관리 프로그램 — 현재 버전. `WORKLOG.md` 참고 |
| `VProEditor01/` | VisionPro Job 편집기 (QuickBuild 독립형) |
| `VProEditorGroup1/` | VisionPro Job 편집기 (ToolGroup 대응) |
| `VProEditAll/` | vpp 통합 편집기 (QuickBuild / Job / ToolGroup / ToolBlock). `WORKLOG.md` 참고 |

새 프로젝트는 이 폴더 아래에 만들면 루트 `.gitignore` 가 그대로 적용된다.

## Git 에 넣지 않는 것
- 빌드 출력: `bin/`, `obj/`, `.vs/`
- VisionPro 파일·시험 데이터: `*.vpp`, 이미지, `_Data/`
  - vpp 와 시험 이미지는 수십 MB~1GB 이상이라 PC 에만 보관한다 (`D:\10_InProcess\VINA\QB_Jobs` 등).

## 이력
- 2026-11-06: `D:\10_InProcess\VINA` 의 각 프로젝트를 모음. 기존 Git 이력이 있던 프로젝트
  (VProEditAll, VproManageBlack01/02, VProManager01/02) 는 `git subtree` 로 커밋 이력을 보존해 가져옴.
