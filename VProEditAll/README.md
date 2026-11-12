# VProEditAll

VisionPro vpp 파일(QuickBuild / Job / ToolGroup / ToolBlock)을 불러와 편집하고 저장하는 프로그램.
작업 기록과 시험 결과는 `WORKLOG.md` 참고.

---

## 1. 운영 규칙 (사용하는 사람)

1. **출처를 모르는 vpp 파일은 열지 않는다.**
   vpp 파일은 프로그램 코드(스크립트)를 포함할 수 있고, 파일을 여는 과정 자체가 코드 실행에 악용될 수 있다.
   메일·USB 로 받은 파일은 만든 사람을 확인한 뒤 연다.
2. **장비 PC 에서는 QuickBuild / 생산 프로그램을 먼저 종료한다.**
   같은 카메라를 두 프로그램이 동시에 쓰면 연결이 끊어진다.
3. **카메라는 편집기를 종료해야 해제된다.** 편집이 끝나면 편집기를 끄고 생산 프로그램을 실행한다.
4. **편집 화면의 실행/루프 버튼은 실제 장비를 움직인다.**
   Image Source 를 실행하면 실제 카메라로 영상을 취득한다.
   I/O 가 켜진 파일(상태줄 "I/O 사용: 켜짐")은 실행하지 않는다. (I/O 가 켜진 파일은 아직 시험하지 못함)
5. **시험은 사본으로 한다.** 운영 중인 원본 vpp 로 시험하지 않는다.
6. 저장하면 이전 파일이 `.bak` 으로 남는다. **.bak 은 1개만 남고 저장할 때마다 덮어써진다.**

---

## 2. 코드 읽는 순서 (처음 보는 사람)

| 순서 | 파일 | 하는 일 |
|---|---|---|
| 1 | `Program.cs` | 프로그램 시작, 오류 처리 방법 정하기 |
| 2 | `MainForm.cs` | 화면과 버튼 동작. 안의 `#region` 1~5 순서대로 읽는다 |
| 3 | `VppDocument.cs` | 불러온 파일 하나: 불러오기 / 편집 대상 꺼내기 / 저장 / 정리 |
| 4 | `VppInspector.cs` | 파일을 열기 전에 바이트로 형식·카메라·I/O 확인 |
| - | `Button3D.cs` | 버튼 그림 그리기. 동작과 관계없으므로 건너뛰어도 된다 |

[로드] 버튼을 누르면 일어나는 일:

```
MainForm.BtnLoad_Click
 ├─ 열린 파일 저장 확인            (MainForm.ConfirmCloseCurrent)
 ├─ 파일 선택
 ├─ 바이트로 형식/카메라/I/O 확인  (VppInspector.Inspect)        ← VisionPro 로 열지 않음
 ├─ 카메라/I/O 경고               (MainForm.ConfirmHardwareWarning)
 ├─ 이전 파일 정리                (MainForm.CloseCurrent)
 ├─ VisionPro 로 불러오기          (VppDocument.Load)             ← 여기서 카메라 연결 시도
 └─ 편집 화면 연결 / 결과 표시     (MainForm.FillJobs → BindEditor)
```

---

## 3. 바꾸면 안 되는 것 (코드를 고치는 사람)

코드에 `[안전]` 이라고 적힌 곳이다. 쉽게 고치더라도 **아래 순서와 동작은 그대로** 둔다.

| # | 내용 | 위치 | 바꾸면 생기는 일 |
|---|---|---|---|
| 1 | 바이트 확인 → 경고 → 불러오기 순서 | `MainForm` 불러오기 | 경고 전에 카메라·I/O 에 연결됨 |
| 2 | 정리 순서: 편집 화면 Subject 끊기 → 정지 → Shutdown/Dispose → 편집 화면 해제 | `MainForm.CloseCurrent`, `VppDocument.Dispose` | 정리 중 오류 |
| 3 | 저장: 임시 파일 → 원본과 교체 → .bak | `VppDocument.Save` | 저장 실패 시 원본 손상 |
| 4 | 프로그램 종료 시 카메라 Disconnect | `MainForm.ReleaseCameras` | R6025 오류창이 뜨고 멈춤 |
| 5 | 정리·종료 중 오류 무시(`catch { }`) | `StopRunning`, `ReleaseObject`, `ReleaseCameras`, `FormClosing` | 중간 오류로 카메라 해제가 건너뛰어짐 |
| 6 | ToolBlock 을 ToolGroup 보다 먼저 판정 | `VppInspector.KindFromObject` | ToolBlock 이 ToolGroup 으로 판정됨 |
| 7 | 편집 화면 툴바의 열기/저장/다른 이름으로 저장 숨김 | `MainForm.HiddenButtonTips` | 백업 없이 저장되거나 QuickBuild 파일이 ToolGroup 으로 바뀜 |
| 8 | 바이트 확인(`VppInspector`)은 VisionPro 로 파일을 열지 않음 | `VppInspector` | 경고가 의미 없어짐 |

코드를 고친 뒤에는 `WORKLOG.md` 의 시험 항목(불러오기 4종류 / 저장·백업 / Minimum / 종료 후 카메라 해제)을 **사본으로** 다시 확인한다.

---

## 4. 저장소 주의

- 이 저장소는 GitHub 공개 저장소다. 장비 정보·고객 정보·내부 경로를 문서에 적지 않는다.
- vpp·이미지·백업 파일은 `.gitignore` 로 커밋에서 빠진다. 이 설정을 지우지 않는다.
