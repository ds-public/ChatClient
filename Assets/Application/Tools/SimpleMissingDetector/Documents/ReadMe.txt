◆Simple Missing Detctor の使い方

　Version 2021/01/22

-------------------------------------------------------------------------------
●概要

　UnityEditor の Project タグ内の Asset 群、または Hierarchy タグ内の GameObject 群の中で、
　参照が失われている GameObject Component Property を検出して表示します。

-------------------------------------------------------------------------------
●出来る事と出来ない事(UnityEditor の仕様による機能制限)

　・シーンファイル(*.unity)内を直接検査する事はできません。
　　　→※シーンファイルは、Hierarchy に展開(実体化)して検査する必要があります。

　・プレハブファイル(*.prefab)内の、
　　他のプレハブ(GameObject)に対する参照状態を検査する事はできません。
　　　→※プレハブ内の他のプレハブ(GameObject)への参照を検査するには、
　　　　　Hierarchy に展開(実体化)して検査する必要があります。

　　　　　ただし、
　　　　　　・Component への参照状態は、プレハプファイル(*.prefab)のまま可能です。
　　　　　　・そのプレハブファイル(*.prefab)が含まれるシーンファイル(*.unity)を、
　　　　　　　Hierarcy に展開した場合、参照の失われてる他のプレハブ(GameObject)には、
　　　　　　　ダミーのプレハブ(GameObject)が自動生成されてしまうため、
　　　　　　　正しく Missing の検査ができなくなります。
　　　　　　　　→※Hierarchy には、シーン(*.unity)ではなくプレハブ(*.prefeb)を展開すること。

-------------------------------------------------------------------------------
●機能説明

　[Project] ダブ

　　UnityEditor の Project タブに表示されている、
　　各 Asseet ファイル群内の、Missing を検出します。

　[Hierarcjy] タブ

　　UnityEditor の Hierarchy タブに表示されている、
　　各 GameObject 階層構造内の、Missing を検出します。


　※Missing (対象への参照が失われた状態) として検出できるものは以下のものです。

　　・プレハブ内の他のプレハブ(GameObject)への参照
　　　※[Hierarchy]限定

　　・GameObject 内の Component への参照
　　　※[Project][Hierarchy]両方

　　・Component 内の Property の他の Asset への参照
　　　※[Project][Hierarchy]両方

---------------------------------------

○[Project] ダブ内の機能

　UnityEditor の Project タブ内の Asset ファイル群内で発生している
　Missing を検出するために使用します。

　[Root AssetPath]
　　…Missing を検出したい UnityEditor の Project タブ内の
　　　フォルダまたは Asset ファイルを選択します。

　　　フォルダを選択した場合は、
　　　そのフォルダ以下のサブフォルダと Asset ファイル全ての Missing 検査を行います。

　Asset Filtering
　　…Missing の検査対象とする Asset ファイルの種類の絞り込みを行います。
　　　※基本はデフォルトの状態で問題はない

　Property Masking
　　…Missing の検出を不要とする参照対象の Asset の種別を指定します。
　　　必要に応じてチェックを入れて下さい。

　　　※ParticleSystemRenderer 内の Mesh の参照など、
　　　　対応の必要がほとんど無い、軽微な Missing が検出されるのを除外する、
　　　　といった用途で使用します。

　[Refresh]
　　…[Root AssetPath] で指定した対象に対して、再度 Missing の検査を行います。

　□検出された Missing 情報一覧

　　Component
　　　…Property で Missing が発生している Component を表示しています。
　　　　ダブルクリックで対象の Asset ファイルにジャンプします。

　　Property
　　　…Missing を起こしている Property の種別を表し、
　　　　その詳細な情報を表示します。

　　Link
　　　…Missing を起こしている Asset ファイルにジャンプします。

　　AssetPath
　　　…Missing を起こしている Asset ファイルの Project 内のパスを表示しています。

---------------------------------------

○[Hierarchy] ダブ内の機能

　UnityEditor の Hierarcy タブ内の GameObject 階層構造内で発生している
　Missing を検出するために使用します。

　[Root GameObject]
　　…Missing を検出したい UnityEditor の Hierarchy タブ内の
　　　GameObject を選択します。

　　　選択した GameObject とその子に属する全ての GameObject に対して、
　　　Missing の検査を行います。

　Property Masking
　　…Missing の検出を不要とする参照対象の Asset の種別を指定します。
　　　必要に応じてチェックを入れて下さい。

　　　※ParticleSystemRenderer 内の Mesh の参照など、
　　　　対応の必要がほとんど無い、軽微な Missing が検出されるのを除外する、
　　　　といった用途で使用します。

　[Refresh]
　　…[Root GameObject] で指定した対象に対して、再度 Missing の検査を行います。

　□検出された Missing 情報一覧

　　Component
　　　…Property で Missing が発生している Component を表示しています。
　　　　ダブルクリックで対象の GameObejct にジャンプします。

　　Property
　　　…Missing を起こしている Property の種別を表し、
　　　　その詳細な情報を表示します。

　　Link
　　　…Missing を起こしている GameObject にジャンプします。

　　GameObjectath
　　　…Missing を起こしている GameObject の Hierarchy 内のパスを表示しています。
