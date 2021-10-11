using Assets.Scripts.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class JigsawPlayground : MonoBehaviour
{
    public static JigsawPlayground Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Seviye bilgisi.
    /// </summary>
    public LevelEditorModel LevelData { get; private set; }

    [Header("Sonlandýrýldý mý? Oyun bir þekilde bitti ise artýk tutma iþlemini ve diðer iþlemleri bitireceðiz.")]
    public bool IsFinalized;

    [Header("Týklamalarý hesaplamak için kullanýlacak çözünürlük.")]
    public Canvas PlaygroundCanvas;

    [Header("Hint controller.")]
    public LevelHintController LHC;

    [Header("Oyun alanýna yerleþtirilecek puzzle parçasý.")]
    public GameObject JigsawPlaygroundItem;

    [Header("Parçalarýn yerleþtirileceði alan.")]
    public RectTransform JigsawPlaygroundContent;

    [Header("Parçalarýn çizimleri.")]
    public Sprite[] Pieces;

    [Header("Toplam satýr sayýsý.")]
    public int RowCount;

    [Header("Toplam sütun sayýsý.")]
    public int ColCount;

    [Header("Her parça arasýndaki fark.")]
    public float PerOffset;

    [Header("Parçalarýn oluþturulacaðý alan.")]
    public RectTransform SpawnArea;

    [Header("Seviyeye ait parçalar.")]
    public List<JigsawPlaygroundItemController> LevelPieces;

    [Header("Seçimi yapýlan ana parça.")]
    public JigsawPlaygroundItemController SelectedPiece;

    [Header("Seçili olan parçalar. Ana parça da dahil.")]
    public List<JigsawPlaygroundItemController> SelectedPieces;

    /// <summary>
    /// We use the generator to generate same random level.
    /// </summary>
    public System.Random Randomizer { get; set; }

    /// <summary>
    /// Mouse position to drag and drop.
    /// </summary>
    private Vector2 selectedPieceStartMPos { get; set; }

    private void Start()
    {
        PlaygroundCanvas = GameObject.FindGameObjectWithTag("GUI").GetComponent<Canvas>();
    }

    public void LoadLevel(LevelEditorModel levelData, Texture2D levelTexture)
    {
        // We create a randomizer.
        Randomizer = levelData.GetRandom();

        // Seviye bilgisini atýyoruz.
        this.LevelData = levelData;

        // Parça sayacý.
        int i = 0;

        // Oyun alanýn boyutlarý.
        Rect sizeOfContent = JigsawPlaygroundContent.GetComponent<RectTransform>().rect;

        // Satýrlarý dönüyoruz.
        for (int r = RowCount - 1; r >= 0; r--)
        {
            // Sütunlarý dönüyoruz.
            for (int c = 0; c < ColCount; c++)
            {
                // Parça ikonunu buluyoruz.
                Sprite piece = Pieces[i];

                // Konumlandýrma.
                float posX = -sizeOfContent.width / 2 + PerOffset * c + 23.75f;
                float posY = -sizeOfContent.height / 4f + PerOffset * r + 23.75f;

                // Parçalarý oluþturuyoruz.
                GameObject jigsawItem = Instantiate(JigsawPlaygroundItem, JigsawPlaygroundContent);
                jigsawItem.GetComponent<RectTransform>().anchoredPosition = GetGivenPosition(c, r);

                // Çizimi ve konumlandýrmasýný yapýyoruz.
                jigsawItem.transform.Find("Puzzle").GetComponent<Image>().sprite = piece;
                jigsawItem.transform.Find("Outline").GetComponent<Image>().sprite = piece;

                // Parçanýn görüntüleneceði alanaý hespalýyoruz..
                RawImage pieceBack = jigsawItem.transform.Find("Puzzle/Back").GetComponent<RawImage>();
                pieceBack.texture = levelTexture;
                pieceBack.GetComponent<RectTransform>().anchoredPosition = GetGivenPosition(c, r) * -1;
                pieceBack.SetNativeSize();

                // Ýndeksi bir arttýrýyoruz bir sonraki parçayý sýrasýyla yüklemek için.
                i++;

                // Komþularýný belirliyoruz.
                JigsawPlaygroundItemController jigsawItemComp = jigsawItem.GetComponent<JigsawPlaygroundItemController>();
                jigsawItemComp.SetGridData(r, c);

                // Parçalarý listeye ekliyoruz.
                LevelPieces.Add(jigsawItemComp);
            }
        }

        // Rastgele sýra ile sýralamalarýný güncelliyoruz.
        foreach (JigsawPlaygroundItemController levelPiece in LevelPieces.OrderBy(x => Randomizer.NextDouble()))
        {
            // UI sýralamasýný güncelliyoruz rastgele olarak.
            levelPiece.GetComponent<Canvas>().sortingOrder = 1;
        }

    }

    public void CheckForNeighborConnections()
    {
        // Maksimum etkileþim mesafesi.
        float interactionOffset = PerOffset * 1.25f;

        // Etkileþim mesafesi. En az olabilecek mesafe.
        float interactionSide = interactionOffset * .5f;

        // Etkileþim karþý kenar mesafe kontorlü. örn; sað ve sol komþu için bu deðer üst ve alt komuþlarýn kontrolü için kullanýlacak.
        float interactionReverseSide = interactionOffset * .15f;

        // Ýlk baðlantý.
        JigsawPlaygroundItemController firstConnection = null;

        // Tüm seçimlerin komþularýný kontrol ediyoruz.
        foreach (JigsawPlaygroundItemController sPiece in this.SelectedPieces.OrderByDescending(x => x == this.SelectedPiece))
        {
            /** Komþularý alýyoruz. **/
            JigsawPlaygroundItemController rightNeighbor = GetRightNeighbor(sPiece.Row, sPiece.Column);
            JigsawPlaygroundItemController leftNeighbor = GetLeftNeighbor(sPiece.Row, sPiece.Column);
            JigsawPlaygroundItemController topNeighbor = GetTopNeighbor(sPiece.Row, sPiece.Column);
            JigsawPlaygroundItemController bottomNeighbor = GetBottomNeighbor(sPiece.Row, sPiece.Column);

            // Sað taraftaki komþusu.
            if (rightNeighbor)
            {
                // Ýki parça arasýndaki mesafeleri alýyoruz.
                float xDiff = rightNeighbor.RectTransform.anchoredPosition.x - sPiece.RectTransform.anchoredPosition.x;
                float yDiff = Mathf.Abs(rightNeighbor.RectTransform.anchoredPosition.y - sPiece.RectTransform.anchoredPosition.y);

                // Mesafe etkileþim mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, rightNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi diðerine saðdan mý dokunuyor?
                    // Ayný zamanda yukarý ve aþaðýdan da mesafeyi ölçüyoruz ki uzak mesafelerde de birbirlerine baðlanmasýnlar.
                    if (xDiff >= interactionSide && xDiff <= interactionOffset && yDiff >= 0 && yDiff <= interactionReverseSide)
                    {
                        // Parçalarý birbirlerine baðlýyorlar.
                        sPiece.ConnectToPiece(rightNeighbor);

                        // Ýlk baðlanýlan parça.
                        if (firstConnection == null)
                            firstConnection = rightNeighbor;
                    }
                }
            }

            // Sol taraftaki komþusu.
            if (leftNeighbor)
            {
                // Ýki parça arasýndaki mesafeleri alýyoruz.
                float xDiff = sPiece.RectTransform.anchoredPosition.x - leftNeighbor.RectTransform.anchoredPosition.x;
                float yDiff = Mathf.Abs(sPiece.RectTransform.anchoredPosition.y - leftNeighbor.RectTransform.anchoredPosition.y);

                // Mesafe etkileþim mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, leftNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi diðerine soldan mý dokunuyor?
                    // Ayný zamanda yukarý ve aþaðýdan da mesafeyi ölçüyoruz ki uzak mesafelerde de birbirlerine baðlanmasýnlar.
                    if (xDiff >= interactionSide && xDiff <= interactionOffset && yDiff >= 0 && yDiff <= interactionReverseSide)
                    {
                        // Parçalarý birbirlerine baðlýyorlar.
                        sPiece.ConnectToPiece(leftNeighbor);

                        // Ýlk baðlanýlan parça.
                        if (firstConnection == null)
                            firstConnection = leftNeighbor;
                    }
                }
            }

            // Üst komþusunu kontrol ediyoruz.
            if (topNeighbor)
            {
                // Ýki parça arasýndaki mesafeleri alýyoruz.
                float xDiff = Mathf.Abs(sPiece.RectTransform.anchoredPosition.x - topNeighbor.RectTransform.anchoredPosition.x);
                float yDiff = sPiece.RectTransform.anchoredPosition.y - topNeighbor.RectTransform.anchoredPosition.y;

                // Mesafe etkileþim mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, topNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi diðerine yukarýdan mý dokunuyor?
                    // Ayný zamanda saðdan ve soldan da mesafeyi ölçüyoruz ki uzak mesafelerde de birbirlerine baðlanmasýnlar.
                    if (yDiff >= interactionSide && yDiff <= interactionOffset && xDiff >= 0 && xDiff <= interactionReverseSide)
                    {
                        // Parçalarý birbirlerine baðlýyorlar.
                        sPiece.ConnectToPiece(topNeighbor);

                        // Ýlk baðlanýlan parça.
                        if (firstConnection == null)
                            firstConnection = topNeighbor;
                    }
                }
            }

            // Alt komuþusunu kontrol ediyoruz.
            if (bottomNeighbor)
            {
                // Ýki parça arasýndaki mesafeleri alýyoruz.
                float xDiff = Mathf.Abs(bottomNeighbor.RectTransform.anchoredPosition.x - sPiece.RectTransform.anchoredPosition.x);
                float yDiff = bottomNeighbor.RectTransform.anchoredPosition.y - sPiece.RectTransform.anchoredPosition.y;

                // Mesafe etkileþim mesafesinden az ise bu iki birim birbirlerine dokunuyor demektir.
                if (Vector2.Distance(sPiece.RectTransform.anchoredPosition, bottomNeighbor.RectTransform.anchoredPosition) <= interactionOffset)
                {
                    // Ancak kontrol etmemiz gereken bu iki birimden birisi diðerine aþaðýdan mý dokunuyor?
                    // Ayný zamanda saðdan ve soldan da mesafeyi ölçüyoruz ki uzak mesafelerde de birbirlerine baðlanmasýnlar.
                    if (yDiff >= interactionSide && yDiff <= interactionOffset && xDiff >= 0 && xDiff <= interactionReverseSide)
                    {
                        // Parçalarý birbirlerine baðlýyorlar.
                        sPiece.ConnectToPiece(bottomNeighbor);

                        // Ýlk baðlanýlan parça.
                        if (firstConnection == null)
                            firstConnection = bottomNeighbor;
                    }
                }
            }
        }

        // Eðer baðlantý yok ise kendisini seçiyoruz.
        if (firstConnection == null)
            firstConnection = this.SelectedPiece;

        // Tüm parçalarý yeniden konumlandýrýyoruz.
        foreach (JigsawPlaygroundItemController piece in this.SelectedPieces.OrderByDescending(x => x == firstConnection))
            piece.RectTransform.anchoredPosition = firstConnection.RectTransform.anchoredPosition + GetPositionConnectedJigsawPuzzle(firstConnection.Row, firstConnection.Column, piece.Row, piece.Column);
    }

    public void ContinueToLevel()
    {
        // We get the history data.
        GameHistoryModel historyValue = GameHistoryController.Instance.GetHistory(this.LevelData, GameDifficulities.JigsawPuzzle);

        // We loop all the items.
        foreach (GameHistoryPieceModel piece in historyValue.HistoryPieces)
        {
            // We are looking for the same piece from previous level.
            JigsawPlaygroundItemController pieceInLevel = LevelPieces.Find(x => x.Row == piece.Row && x.Column == piece.Col);

            // We make sure piece exists in map.
            if (pieceInLevel != null)
            {
                // We load the piece position from the cache.
                pieceInLevel.RectTransform.anchoredPosition = new Vector2(piece.X, piece.Y);
            }
        }

        // We check all the neighbors for all the pieces.
        historyValue.HistoryPieces.ForEach(e =>
        {
            // We are looking for the same piece from previous level.
            JigsawPlaygroundItemController pieceInLevel = LevelPieces.Find(x => x.Row == e.Row && x.Column == e.Col);

            // We make sure piece exists.
            if (pieceInLevel != null)
            {
                // We select the piece.
                OnSelectPiece(pieceInLevel);

                // And check for its connections.
                CheckForNeighborConnections();

                // We deselect piece.
                OnDeselectPiece();
            }
        });
    }

    public JigsawPlaygroundItemController GetRightNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow && x.Column == pieceCol + 1);
    }
    public JigsawPlaygroundItemController GetTopNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow - 1 && x.Column == pieceCol);
    }
    public JigsawPlaygroundItemController GetBottomNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow + 1 && x.Column == pieceCol);
    }
    public JigsawPlaygroundItemController GetLeftNeighbor(int pieceRow, int pieceCol)
    {
        return LevelPieces.Find(x => x.Row == pieceRow && x.Column == pieceCol - 1);
    }
    public Vector2 GetPositionConnectedJigsawPuzzle(int frow, int fcol, int trow, int tcol)
    {
        int rowDiff = trow - frow;
        int colDiff = tcol - fcol;
        return new Vector2(colDiff * PerOffset, rowDiff * PerOffset);
    }

    public void SelectReleatedPieces(JigsawPlaygroundItemController piece)
    {
        // TÜm baðlý olan parçalarý dönüyoruz.
        piece.ConnectedJigsaws.ForEach(e =>
        {
            // Eðer seçim listesinde yok ise ekleyeceðiz.
            if (!SelectedPieces.Contains(e))
            {
                // Seçim listesine ekliyoruz.
                SelectedPieces.Add(e);

                // VE bu baðlý olan parçanýn diðer baðlý olan parçalarýný dönüyoruz.
                SelectReleatedPieces(e);
            }
        });
    }
    public void OnSelectPiece(JigsawPlaygroundItemController piece)
    {
        // Eðer sonlandýrýlmýþ ise geri dön.
        if (IsFinalized)
            return;

        // Seçimleri tutuyoruz.
        this.SelectedPiece = piece;
        this.selectedPieceStartMPos = Input.mousePosition;

        // Önceki listenini temizlendiðinden emin oluyoruz.
        SelectedPieces.Clear();

        // Seçili parçalarý listeye ekliyoruz.
        SelectedPieces.Add(piece);

        // Seçilen birim ile baðlý olan tüm parçalarý seçiyoruz.
        SelectReleatedPieces(piece);

        // Tüm seçimlerin sýralamasýný yukarý alýyoruz ki parçalar altta kalmasýn.
        SelectedPieces.ForEach(e => e.GetComponent<Canvas>().sortingOrder = 2);
    }
    public void OnDeselectPiece()
    {
        // Eðer seçim yok ise geri dönüyoruz.
        if (this.SelectedPiece == null)
            return;

        // Komuþular ile baðlantýlarý olanlarý kontrol ediyoruz.
        CheckForNeighborConnections();

        // Tüm seçimlerin sýralamasýný yukarý alýyoruz ki parçalar altta kalmasýn.
        SelectedPieces.ForEach(e => e.GetComponent<Canvas>().sortingOrder = 1);

        // Seçim listesini temizliyoruz.
        this.SelectedPieces.Clear();

        // Ve seçimi kaldýrýyoruz.
        this.SelectedPiece = null;

        // Bir adým azaltýyoruz ve kayýt ediyoruz ve devamýnda ui yenileniyor.
        SaveLoadController.Instance.SaveData.ActionScore--;
        SaveLoadController.Instance.Save();
        CurrentLevelGameViewController.Instance.RefreshUI();

        // Level pieces.
        List<GameHistoryPieceModel> pieces = LevelPieces.Select(x => new GameHistoryPieceModel(x.RectTransform.anchoredPosition.x, x.RectTransform.anchoredPosition.y, x.Column, x.Row))
            .ToList();

        // We remove history if the level completed.
        GameHistoryController.Instance.SetHistory(this.LevelData, GameDifficulities.JigsawPuzzle, pieces);

        // Oyun sonunu kontrol ediyoruz.
        int totalCombinedPiece = LevelPieces.SelectMany(x => x.ConnectedJigsaws.Select(y => y)).Distinct().Count();

        // Eðer toplam adete eriþiyor ise.
        if (totalCombinedPiece == LevelPieces.Count)
            FinalizePlayground();
    }

    private void FinalizePlayground()
    {
        // Eðer sonlandýrýlmamýþ ise sonlandýrýyoruz.
        if (!IsFinalized)
        {
            // Sonlandýrýldý olarak iþaretliyoruz.
            IsFinalized = true;

            // Current level information.
            LevelEditorModel currentLevelData = CurrentLevelGameViewController.Instance.LevelData;

            // We are updating max level if it is smaller.
            if (CurrentLevelGameViewController.Instance.CurrentLevel == SaveLoadController.Instance.SaveData.MaxReachedLevel)
                SaveLoadController.Instance.SaveData.MaxReachedLevel = CurrentLevelGameViewController.Instance.CurrentLevel + 1;

            // We are reducing one point.
            SaveLoadController.Instance.SaveData.ActionScore += currentLevelData.ScoreOnWin;

            // We are applying changes.
            SaveLoadController.Instance.Save();

            // And we are reducing from the ui.
            CurrentLevelGameViewController.Instance.RefreshUI();

            // We are updating buttons states.
            CurrentLevelGameViewController.Instance.CheckPrevAndNextButtons();

            // We remove history if the level completed.
            GameHistoryController.Instance.RemoveHistory(this.LevelData, GameDifficulities.JigsawPuzzle);
        }
    }

    private void LateUpdate()
    {
        // Eðer bir parçaya baðlý ise.
        if (Input.GetMouseButton(0))
        {
            // Eðer adet kalmadýysa geri dön.
            if (this.SelectedPieces.Count > 0)
            {
                // Farenin bir önceki konumu ile arasýndaki fark.
                Vector2 diff = (Vector2)Input.mousePosition - selectedPieceStartMPos;

                // Bu deðer aslýnda canvas scale ratesi. Kaymalarý önlemek adýna ayný oranda olmalý.
                diff.x /= PlaygroundCanvas.transform.localScale.x;
                diff.y /= PlaygroundCanvas.transform.localScale.y;

                // Sað taraf sýnýr kontorlü yapýyoruz.
                if (diff.x > 0)
                {
                    // Taþýnan parçalardan en uçtakini buluyoruz.
                    float mostRightPieceX = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.x + PerOffset / 2).Max();

                    // Ve bir sonraki konumunu hesaplýyoruz.
                    mostRightPieceX += diff.x;

                    // Sað sýnýr.
                    float rightBorder = JigsawPlaygroundContent.anchoredPosition.x + JigsawPlaygroundContent.rect.max.x;

                    // Eðer ekranýn dýþýna çýkar ise 0 lýyoruz.
                    if (mostRightPieceX > rightBorder)
                        diff.x += rightBorder - mostRightPieceX;
                }

                // Sol taraf sýnýr kontrolünü yapýyoruz.
                if (diff.x < 0)
                {
                    // En soldaki parçayý alýyoruz.
                    float mostLeftPieceX = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.x - PerOffset / 2).Min();

                    // En ucun yeni konumunu hesapladýk.
                    mostLeftPieceX += diff.x;

                    // Sol sýnýr.
                    float leftBorder = JigsawPlaygroundContent.anchoredPosition.x + JigsawPlaygroundContent.rect.min.x;

                    // Eðer ekranýn dýþýna çýkar ise 0 lýyoruz.
                    if (mostLeftPieceX < leftBorder)
                        diff.x += leftBorder - mostLeftPieceX;
                }

                // Eðer yukarýya doðru ilerlemeye çalýþýyor isek sýnýrýný kontrol ediyoruz.
                if (diff.y > 0)
                {
                    // En yukarýdaki parçayý bulduk.
                    float mostTopPieceY = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.y + PerOffset / 2).Max();

                    // Bir sonraki konumunu hesapladýk.
                    mostTopPieceY += diff.y;

                    // Üst sýnýr.
                    float topBorder = JigsawPlaygroundContent.anchoredPosition.y + JigsawPlaygroundContent.rect.max.y;

                    // Eðer ekranýn dýþýna çýkar ise 0 lýyoruz.
                    if (mostTopPieceY > topBorder)
                        diff.y += topBorder - mostTopPieceY;
                }

                // Eðer aþaðý doðru ilerlemeye çalýþýyor isek sýnýrýný kontrol ediyoruz.
                if (diff.y < 0)
                {
                    // En yukarýdaki parçayý bulduk.
                    float mostBottomPieceY = SelectedPieces.Select(x => x.RectTransform.anchoredPosition.y - PerOffset / 2).Min();

                    // Bir sonraki konumunu hesapladýk.
                    mostBottomPieceY += diff.y;

                    // Alt sýnýr.
                    float bottomBorder = JigsawPlaygroundContent.anchoredPosition.y + JigsawPlaygroundContent.rect.min.y;

                    // Eðer ekranýn dýþýna çýkar ise 0 lýyoruz.
                    if (mostBottomPieceY < bottomBorder)
                        diff.y += bottomBorder - mostBottomPieceY;
                }

                // Tüm seçili olan parçalarýn konumunu güncelliyoruz.
                foreach (JigsawPlaygroundItemController piece in SelectedPieces)
                    piece.RectTransform.anchoredPosition += diff;

                // Ekrana göre konumu güncelliyoruz sapma olmasýn diye.
                diff.x *= PlaygroundCanvas.transform.localScale.x;
                diff.y *= PlaygroundCanvas.transform.localScale.y;

                // Farenin son konumunu güncelliyoruz.
                selectedPieceStartMPos += diff;
            }
        }

        // Eðer seçili ise ve mouse býrakýldý ise seçimi kaldýrýyoruz.
        if (Input.GetMouseButtonUp(0))
        {
            // Seçimi kaldýrdýk.
            JigsawPlayground.Instance.OnDeselectPiece();
        }
    }

    public Vector2 GetRandomPosition()
    {
        // Rastgele x koordinat alan içerisinde.
        float x = SpawnArea.rect.min.x + (float)Randomizer.NextDouble() * (SpawnArea.rect.max.x - SpawnArea.rect.min.x);

        // Rastgele y koordinatý verilen alan içerisinde.
        float y = SpawnArea.rect.min.y + (float)Randomizer.NextDouble() * (SpawnArea.rect.max.y - SpawnArea.rect.min.y);

        // Alaný geri dönüyoruz.
        return new Vector2(x, SpawnArea.rect.position.y - y);
    }

    public Vector2 GetGivenPosition(int col, int row)
    {
        // Rastgele x koordinat alan içerisinde.
        float x = -(PerOffset * ColCount) / 2 + col * PerOffset + PerOffset / 2;

        // Rastgele y koordinatý verilen alan içerisinde.
        float y = -(PerOffset * RowCount) / 2 + row * PerOffset + PerOffset / 2;

        // Alaný geri dönüyoruz.
        return new Vector2(x, y);
    }

}
