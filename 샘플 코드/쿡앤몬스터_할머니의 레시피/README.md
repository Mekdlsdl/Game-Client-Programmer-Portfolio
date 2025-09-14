# 🎮 쿡앤몬스터: 할머니의 레시피
> 이븐아이 게임톤 (구글 플레이 런칭 예정작)

<br>

> [!NOTE]
> 아래는 이름순으로 정렬한 각 폴더와 파일 설명입니다!

<br>
  
## ☁️ Backend : 서버 연동 관련 코드들 - 진행 중
- BackendLogin : 구글 로그인 관련 코드
- BackendManager : 뒤끝 초기화
- BackendRepo : 서버 조회 및 저장(유저 데이터) 관련 코드

<br>
  
## 🛠️ Setup : 정비 씬에서 쓰이는 코드들
- MenuTabController : 메뉴 설정 탭 관련 코드
- RecipeItemView : 메뉴탭 목록 및 레시피 도감 목록 풀링 시 사용하는 코드
- RecipeTapController : 레시피 도감(레시피 정보, 해금 가능한 탭) 관련 코드

<br>

## 🧑🏻‍🍳 Tycoon : 요리 시스템 관련 코드들
- BinManager : 기지(타이쿤 장소) 안의 재료 쓰레기통 관련 코드
- CookingHandler : 조리기구, 요리 관련 코드
- CounterHandler : 손님, 카운터 관련 코드
- InventoryManager : 인벤토리 관련 코드

<br>

### CustomerSystem : 손님 관련 코드들
- Customer : 손님 정보를 담은 코드
- CustomerOrder : 손님의 주문 관리 코드 (주문 확률 관리)
- CustomerPool : 손님 풀링 관리 코드
- CustomerSpawner : 손님 스폰 관련 코드

<br>

### InteractionButton : 타이쿤 푸시 버튼 관련 코드들
- CookAction : 완성된 음식을 픽업하는 액션 관리
- IInteractAction : 액션들 인터페이스
- InteractionButtonRouter : 여러 액션 중 가능한 액션 선별 루터
- OrderAction : 손님 레시피 정보 액션 관리
- SwapAction : 음식을 놓고 집는 액션 관리

<br>

### Trigger : 기지(타이쿤 장소) 안의 모든 트리거 관리 코드들
- PlaceTrigger : 트리거 정보
- TriggerHUDListener : 쏘는 이벤트들을 받는 리스너
- TriggerManager : 트리거의 모든 액션, 게터, 세터 관리 코드
- TriggerZone : 트리거를 감지하고 함수로 넣어주는 역할

<br>

## 🗂 그외 코드들
- AdsManager : 광고(애드몹) 연동 세팅 - 진행 중
- Barigate : 기지, 전투 장소 사이 바리게이트 관련 코드
- ChangeSortingOrder : 기지 타일맵 SortingOrder 관련 코드 (ex. 벽 뒤에 플레이어 숨기)
- LobbyManager : 메인화면 기능 연결 관련 코드
