# VproManageBlack02 코드 검토 보고서

2026-10-28 · yhkim@nanocore.co.kr

원본 문서: https://claude.ai/artifact/PUzSaEaF3iLtQaG9x6ngCj

## 개요

검사 결과를 신뢰할 수 없게 만드는 버그 5건이 가장 시급하며, 그 다음이 VisionPro 자원 해제와 예외 처리입니다. 이 문서는 1차·2차 검토를 합친 것이고, 코드는 수정하지 않았으며 빌드·실행도 하지 않았습니다.

- **대상**: `D:\10_InProcess\VINA\VproManageBlack02` — VisionPro QuickBuild VPP를 불러와 카메라 또는 파일 이미지로 검사하는 단일 폼 WinForms 앱(.NET Framework 4.7.2)
- **검토 파일**: `Form1.cs`(621줄, 미커밋 변경 약 440줄 포함), `Form1.Designer.cs`, `Program.cs`, `.csproj`, `.sln`, `App.config`, `AssemblyInfo.cs`, `bin/x64/Debug` 출력물, 직전 커밋 버전
- **제외 항목**: Job 이미지 소스와 `CogAcqFifoTool1`이 같은 카메라를 동시에 여는 문제는 작성자 확인 결과 해당 없음으로 제외했습니다.
- **표기**: 위치는 `파일:줄` 기준이며, 우선순위는 높음·중간·낮음 3단계입니다.

## 1. 치명적: 잘못된 결과가 나올 수 있는 버그

다섯 건 모두 이전 검사값이 현재 결과처럼 표시되거나 프로그램이 종료될 수 있습니다.

| # | 문제 | 위치 | 영향 | 개선 방안 |
| --- | --- | --- | --- | --- |
| 1-1 | `jobManager.Run()`은 비동기인데 직후에 `RunStatus`·`Results`·`Outputs`를 읽음. 실행 중인 Job 스레드와 UI 스레드가 같은 툴에 동시에 접근함 | `Form1.cs:305-335` | 이전 검사값 표시, 간헐적 예외 | 동기 실행 `myToolGroup.Run()`으로 통일하거나, `UserResultAvailable`/`Stopped` 이벤트에서 `BeginInvoke`로 UI 갱신 (VisionPro 문서로 비동기 동작 확인 필요) |
| 1-2 | 파일 선택을 취소하거나 실패해도 `offlineFileMode()`가 `void`라 검사가 계속됨 | `Form1.cs:271-294`, `418-420` | 이전 이미지의 PMAlign·측정값이 PASS로 표시 | `bool` 반환, 실패·취소 시 즉시 return |
| 1-3 | `LEFTside`/`RIGHTside`의 `RunStatus`를 확인하지 않고 `Outputs`를 읽음 | `Form1.cs:548-552` | 측정 실패 시 이전 값 표시 | `RunStatus.Result == Accept` 확인 후 읽기, 실패 시 NG 표시 |
| 1-4 | `getInspectResults()`가 try 밖에서 항상 실행됨 | `Form1.cs:521` | 앞 단계 예외 후 NRE·InvalidCastException으로 앱 종료 | try 안으로 이동, 성공 경로에서만 호출 |
| 1-5 | `jobManager == null`이면 파일 모드도 막힘 (VPP가 CogJob/ToolGroup 형식인 Case B·C) | `Form1.cs:391` | 오프라인 검사 불가 | 모드별 필수 객체만 검사 (오프라인: `myToolGroupGroup`·`IPOneImageTool`, 라이브: `jobManager`·`acqFifoTool`) |

## 2. 자원 관리·안정성

VisionPro 자원을 해제하지 않고 초기화 실패를 막지 않아, 장시간 운영하거나 VPP를 다시 로드할 때 문제가 생길 수 있습니다.

| # | 문제 | 위치 | 개선 방안 |
| --- | --- | --- | --- |
| 2-1 | 종료 시 `jobManager.Shutdown()`을 호출하지 않음. 프로세스가 남거나 카메라가 해제되지 않을 수 있음 | `Form1` (FormClosing 없음) | `FormClosing`에서 `jobManager?.Shutdown()`, 필요하면 `CogFrameGrabbers.Disconnect(false)` |
| 2-2 | VPP를 다시 로드할 때 이전 `jobManager`를 정리하지 않음 | `Form1.cs:135` | 교체 전 기존 Job Shutdown |
| 2-3 | 툴을 못 찾아도 MessageBox만 띄우고 일부만 초기화된 상태로 계속함. `Tools[2]`를 번호로 찾고, `myToolGroupGroup`이 null이면 201행에서 NRE | `Form1.cs:174-221`, `183`, `201` | 툴 이름으로 일괄 검증, 실패 시 롤백하고 `isJobReady` 플래그로 Run 버튼 비활성화 |
| 2-4 | `locate`, `pmAlignTool`, `Results`가 null일 수 있음 (툴 실행 에러 시 `Results` 자체가 null) | `Form1.cs:240-244` | `locate.RunStatus` 먼저 확인 후 null 검사 |
| 2-5 | 검사 중 Run을 다시 누를 수 있음 | `Form1.cs:389` | 실행 중 버튼 비활성화, `WaitCursor` |
| 2-6 | 새로 만든 레코드를 `Clear()`해서 효과가 없는 코드 | `Form1.cs:407-408` | 삭제 |
| 2-7 | VPP 안의 모든 Job을 실행함. Job이 추가되면 의도하지 않은 검사도 실행됨 | `Form1.cs:309` | 사용하는 Job만 지정해서 실행 |
| 2-8 | 전역 예외 처리 없음. 예상 못한 예외에 앱이 종료됨 | `Program.cs` | `Application.ThreadException`, `AppDomain.UnhandledException`에서 로그와 안내 |
| 2-9 | 시작 시 라이선스를 확인하지 않아, 동글이 없으면 VPP 로드에서 원인 불명 예외가 남 | `Program.cs` | 시작 시 라이선스 확인 후 명확한 메시지 |
| 2-10 | VPP 로드 중 UI가 멈추고, 실패 시 `ex.Message`만 표시해 버전 불일치 같은 원인을 알 수 없음 | `Form1.cs:124-228` | 진행 표시·WaitCursor, `InnerException`까지 로그 |

## 3. 검사 로직·기능 보완

측정값을 표시만 하고 OK/NG를 판정하지 않으며, 기준값과 이름이 코드에 하드코딩되어 있습니다.

**검사 로직**

| # | 문제 | 위치 | 개선 방안 |
| --- | --- | --- | --- |
| 3-1 | 측정값에 OK/NG 판정이 없음 | `Form1.cs:541-558` | 공차(min/max) 비교 후 최종 OK/NG를 크게 표시 |
| 3-2 | PMAlign Score 기준 0.6 하드코딩. 툴의 `AcceptThreshold`와 이중 관리 | `Form1.cs:254` | 설정 파일이나 UI로 분리, 기준 하나로 통일 |
| 3-3 | `Results[0]`만 확인해 결과가 여러 개인 경우(이중 투입, 오검출)를 판단하지 않음 | `Form1.cs:252` | `Results.Count` 기준 추가 |
| 3-4 | `Outputs[0]`처럼 번호로 읽고 `(double)`로 직접 형변환 | `Form1.cs:548-552` | `Outputs["LeftDist1"]`처럼 이름으로 읽고 타입 검사 |
| 3-5 | 레코드 키 `"CogToolGroup1.Locate.CogFixtureTool1.OutputImage"` 하드코딩. 오프라인에서는 하위 그룹만 실행하고 상위 그룹 레코드를 사용 | `Form1.cs:443`, `508` | `locate`나 `fixtureTool`의 `CreateLastRunRecord()`를 직접 사용하고 null 확인 |
| 3-6 | Display1("Input Image")에 입력 이미지를 넣자마자 `IPOneImageTool.OutputImage`로 덮어씀 | `Form1.cs:284-292`, `325-333` | 표시할 이미지를 하나로 정해 정리 |
| 3-7 | Display2에 Fixture 출력만 나오고 LEFTside/RIGHTside 측정 그래픽이 없음 | `Form1.cs:443-446` | 측정 툴 레코드·그래픽도 표시 |
| 3-8 | Locate가 실패해도 그룹 전체가 실행되어 측정 툴까지 돎 | VPP 구조 | Locate 결과에 따라 측정 툴 실행 여부 제어 |
| 3-9 | Dist 단위(pixel/mm) 표시 없음. CalibFix를 참조하지만 캘리브레이션 흐름이 보이지 않음 | `Form1.Designer.cs`, VPP | 단위 표기, 캘리브레이션 적용 여부 확인 |
| 3-10 | 이미지 로드: `.tif`/`.idb`/`.cdb` 미지원, 컬러 이미지 미처리, 취소와 실패가 같은 메시지. 주석은 "Deep Copy"인데 실제로 복사하지 않음 | `Form1.cs:564-608` | 형식 추가, Grey 변환·타입 확인, 취소는 조용히 처리 |

**추가하면 좋은 기능**

- 검사 결과 CSV 저장, NG 이미지 저장, 오류 로그 파일
- 마지막 VPP 경로 저장과 시작 시 자동 로드 (직전 커밋에 있던 시작 시 VPP 선택을 대체)
- 폴더 단위 일괄 검사 (파라미터 변경 후 회귀 검증용)
- 카메라 라이브 영상 보기, 노출·게인·취득 타임아웃 설정
- 양산 적용 시 하드웨어 트리거, 연속 실행, PLC/IO 결과 출력

## 4. 프로젝트 설정·배포

플랫폼 설정과 Cognex 참조가 x86과 x64로 섞여 있어, 현장 PC에 배포할 때 로드 오류가 날 수 있습니다.

| # | 문제 | 위치 | 개선 방안 |
| --- | --- | --- | --- |
| 4-1 | 플랫폼 혼재: `Debug\|AnyCPU`는 x86, `Release\|AnyCPU`는 AnyCPU, x64 구성은 `Prefer32Bit=true`. Cognex HintPath도 `Program Files (x86)`와 `Program Files`가 섞임 | `VproManageBlack02.csproj`, `.sln` | VisionPro 설치 비트수에 맞춰 x64로 통일, AnyCPU 구성 삭제 |
| 4-2 | Cognex DLL 수십 개가 출력 폴더로 복사됨 (VisionPro3D 약 25MB 포함) | `bin/x64/Debug` | 참조를 `Private=False`(Copy Local 끄기)로 바꿔 설치된 런타임 사용 |
| 4-3 | 출력 폴더의 Cognex DLL 날짜가 2015년 | `bin/x64/Debug` | 현장 PC에 설치된 VisionPro 버전, VPP를 저장한 버전과 일치하는지 확인 |
| 4-4 | QuickBuild 내부 API인 `Implementation.Internal` 네임스페이스 사용, 쓰지 않는 using 다수 | `Form1.cs:1-20` | 제거 |
| 4-5 | 버전 `1.0.0.0`, 회사명 비어 있음, 저작권 2025 | `Properties/AssemblyInfo.cs` | 버전 규칙을 정하고 결과 로그에 버전 기록 |
| 4-6 | 사용할 VPP 파일명, 필수 툴 이름, VisionPro 버전이 주석에만 있음 | `Form1.cs:24-29` | README로 문서화 |
| 4-7 | 직전 커밋 이후 `Form1.cs` 변경 약 440줄이 커밋되지 않음 | Git | 수정을 시작하기 전에 현재 상태 커밋 |

## 5. UI·사용성

결과값을 작업자가 고칠 수 있고, 현재 상태가 화면에 보이지 않는 점이 가장 큽니다.

| # | 문제 | 위치 | 개선 방안 |
| --- | --- | --- | --- |
| 5-1 | 결과 TextBox에 `ReadOnly`가 없어 측정값을 직접 고칠 수 있음 | `Form1.Designer.cs:127-165` | `ReadOnly = true` |
| 5-2 | 로드된 VPP와 현재 모드(파일/카메라)가 표시되지 않음. 폼 제목이 "Form1" | `Form1.Designer.cs:208` | 제목줄이나 상태바에 표시, 검사 시간(ms)과 누적 OK/NG 카운트 추가 |
| 5-3 | 오류마다 MessageBox를 띄워 연속 검사를 막음 | `Form1.cs` 전반 | 상태바나 로그창으로 대체 |
| 5-4 | CogRecordDisplay를 코드에서 절대 좌표(600×600)로 배치 | `Form1.cs:67-79` | Designer에 배치하거나 `TableLayoutPanel`과 Anchor/Dock 사용 |
| 5-5 | 클라이언트 영역 1668×903 고정, DPI 대응 없음. 노트북·고배율 모니터에서 잘리거나 흐려짐 | `Form1.Designer.cs:191` | DPI 인식 설정, 크기 조절 가능한 레이아웃 |
| 5-6 | 굴림 고정 글꼴과 기본 글꼴 혼용, 측정값 글꼴이 작음 | `Form1.Designer.cs` | 글꼴 통일, 결과값 크게 표시 |
| 5-7 | 개인 PC 경로가 `InitialDirectory`에 하드코딩 | `Form1.cs:572` | 마지막 사용 폴더를 설정에 저장 |
| 5-8 | 오타: "Result mage", "측저", "Reults", "Disply" | `Form1.Designer.cs:89`, `Form1.cs:397`, `520`, `539` | 수정 |

## 6. 코드 품질·유지보수

VisionPro 처리와 UI가 한 클래스에 섞여 있고, 이름과 주석이 코드의 실제 역할을 반영하지 못합니다.

- **이름 정리**: 검사 실행 핸들러가 `button1_Click`, 모델 로드 핸들러가 `button1_Click_1`입니다. 이벤트 핸들러가 아닌 메서드가 `btnLoadJob_Click`이고, 컨트롤은 `textBox1~5`, `label1~7`입니다. 예시: `btnRun`, `btnLoadModel`, `txtLeftDist1`, `txtAlignScore`.
- **쓰지 않는 코드**: 필드 `fixtureTool`·`cogDisplayOut`, 주석 처리된 `tbImageSource`, 빈 핸들러 `chkFileMode_CheckedChanged`, 디버그용 주석 블록(`Form1.cs:354-381`, `469-492`). 이력은 Git에 맡기고 지웁니다.
- **책임 분리**: `isCheckLocate()`가 판정과 UI 갱신(`textBox5`)을 함께 합니다. VisionPro 로드·실행·결과 추출은 `VisionService` 같은 별도 클래스로 분리합니다.
- **이력 관리**: 날짜 주석(`261028@jdin`) 대신 Git 커밋 메시지를 사용합니다.
- **예외 기록**: 예외 메시지만 보여주고 스택 정보는 버립니다. NLog나 log4net 같은 파일 로그로 남깁니다.
- **회귀 테스트**: 기준 이미지와 기대값을 짝지은 테스트가 있으면 VPP를 바꾼 뒤 결과 변화를 바로 확인할 수 있습니다.

## 7. 우선순위 개선 로드맵

먼저 현재 상태를 커밋하고, 결과 신뢰성(1단계)을 확보한 뒤 안정성과 기능을 순서대로 보완하는 것을 권장합니다.

| 단계 | 우선순위 | 항목 | 참조 |
| --- | --- | --- | --- |
| 0 | 높음 | 현재 변경분 커밋 | 4-7 |
| 1 | 높음 | 실행 방식을 동기 실행 또는 이벤트 방식 하나로 통일 | 1-1, 2-7 |
| 1 | 높음 | 파일 취소·실패 시 검사 중단 | 1-2 |
| 1 | 높음 | 모든 툴의 `RunStatus`·null 확인 | 1-3, 2-4 |
| 1 | 높음 | `getInspectResults` 예외 처리, 모드별 초기화 점검 | 1-4, 1-5 |
| 2 | 높음 | `Shutdown`과 이전 Job 정리, VPP 로드 검증·롤백 | 2-1, 2-2, 2-3 |
| 2 | 높음 | 전역 예외 처리와 파일 로그 | 2-8, 6 |
| 2 | 중간 | x64 통일, Cognex DLL 복사 끄기, 버전 확인 | 4-1, 4-2, 4-3 |
| 3 | 중간 | OK/NG 판정, 기준값 설정화, 이름 기반 출력 읽기 | 3-1, 3-2, 3-4 |
| 3 | 중간 | 결과 TextBox `ReadOnly`, 측정 그래픽 표시, 상태 표시 | 5-1, 3-7, 5-2 |
| 3 | 중간 | 결과 CSV·NG 이미지 저장, 마지막 VPP 자동 로드 | 3 |
| 4 | 낮음 | 이름·오타·주석 정리, 레이아웃·DPI | 5, 6 |
| 4 | 낮음 | `VisionService` 분리, 회귀 테스트, 폴더 일괄 검사 | 3, 6 |
| 5 | 필요 시 | 하드웨어 트리거, 연속 실행, PLC/IO 연동 | 3 |
