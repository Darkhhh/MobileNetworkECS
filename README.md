# MobileNetworkECS

Этот фреймворк сделан под вдохновением от [LeoECS Lite](https://github.com/Leopotam/ecslite), с упором на те же принципы: 
- независимость от игровых движков,
- возможность использовать фреймворк в любых программах на C#,
- минимизация выделения памяти,
- быстродействие.

## Содержание

- [Установка](#---------)
- [Основные понятия](#----------------)
  + [Сущность](#--------)
  + [Компонента](#----------)
  + [Система](#-------)
- [Как использовать?](#-----------------)
  + [EcsPool](#ecspool)
  + [Entity](#entity)
  + [EcsFilter](#ecsfilter)
    * [Просмотр сущностей в фильтре](#----------------------------)
  + [EcsSystem](#ecssystem)
- [Предупреждения](#--------------)

## Установка

Для использования фреймворка в проекте достаточно скачать архив со страницы релизов и добавить исходники в проект.

## Основные понятия

### Сущность

Представляет собой обычный `int`. Выступает в качестве идентификатора для доступа к данным.

### Компонента

Представляет собой контейнер с данными. Наличие логики внутри компоненты остается на совести разработчика.

```csharp
public struct SampleComponent
{
    public int SomeData1;
    public bool SomeData2;
}
```

> Компоненты обязательно должны быть структурами, но не классами.

### Система

Представляет собой класс, не содержащий никаких данных (кроме, возможно, вспомогательных для функционирования) и занимающийся исключительно обработкой отфильтрованных сущностей.
```csharp
public class SampleSystem : IInitSystem, IRunSystem, IPostRunSystem, IDisposeSystem
{
    public void Init(IEcsWorld world)
    {
        // Do something...
    }

    public void Run(IEcsWorld world)
    {
        // Do something...
    }

    public void PostRun(IEcsWorld world)
    {
        // Do something...
    }

    public void Dispose(IEcsWorld world)
    {
        // Do something...
    }
}
```



## Как использовать?

Для работы с фреймворком необходимо создать корневой объект типа `IEcsWorld`:
```csharp
var world = new EcsWorld();
```

Он представляет собой контейнер для сущностей, пулов и фильтров.

### EcsPool

Пулы компонент можно добавлять вручную, с помощью функции `BindPool`:
```csharp
world
    .BindPool<SomePool1>()
    .BindPool<SomePool2>();
```

Также пул будет автоматически создаваться, если еще нет, при запросе через `GetPool`:
```csharp
var pool = world.GetPool<SomePool3>();
```
> Пулы, которые объявлены в фильтрах, но не созданы заранее, будут добавлены в мир при сборке фильтра.


### Entity

Создание сущности производится с помощью двух методов:
```csharp
var entityId = world.NewEntityId();
// OR
var entity = world.NewEntity();
```

Оба метода равнозначны, однако имеют разный возвращаемый тип. Метод `NewEntityId()` возвращает числовой идентификатор сущности, 
по которому сущность можно добавлять в пулы, получать по нему значение и удалять из пулов. Однако для этого сначала нужно получить пул из мира:
```csharp
var pool = world.GetPool<SampleComponent>();
var entityId = world.NewEntityId();
ref var component = ref pool.Add(entityId);
```

Метод `NewEntity()`, в свою очередь, возвращает более удобочитаемую обертку над сущностью, благодаря которой код можно немного упростить:
```csharp
var entity = world.NewEntity();
ref var component = ref entity.AddComponent<SampleComponent>();
```

При этом из обертки можно получить идентификатор с помощью `entity.GetId()`, а по идентфикатору можно получить обертку с помощью `world.GetEntityById(entityId)`. 

### EcsFilter

Представляет собой контейнер для сущностей, отфильтрованных по наличию или отсутствию определенных компонент:
```csharp
public class SampleFiltersSystem : IInitSystem
{
    private EcsFilter _filter = null!;
    
    public void Init(IEcsWorld world)
    {
        _filter = new EcsFilter(world);
        _filter.Inc<SampleComponent>().Exc<PlayerComponent>().Register();
    }
}
```
> Для того, чтобы фильтр начал обрабатываться ECS миром, его нужно в нем зарегистрировать с помощью метода `Register()`.

Фильтр достаточно собрать один раз и закешировать, пересборка для обновления списка сущностей не нужна. 
Фильтры поддерживают любое количество требований к компонентам, но один и тот же компонент не может быть в списках "include" и "exclude".

#### Просмотр сущностей в фильтре

Фильтр реализует интерфейсы `IEnumerable` и `IEnumerator`. Благодаря этому по сущностям в фильтре можно пройтись с помощью цикла `foreach`:
```csharp
foreach (int entityId in _filter)
{
    ref var addComponent = ref _pool.Add(entityId);
    ref var getComponent = ref _pool.Get(entityId);
    var hasComponent = _pool.Has(entityId);
    _pool.Remove(entityId);
}
```

> **ВАЖНО!** Метод `Current` возвращает объект типа `object`, поэтому в цикле обязательно приведение к типу `int`.

Также в фильтре можно изменить тип возвращаемого объекта:
```csharp
_filter.EnumerateAsEntity();
foreach (Entity entity in _filter)
{
    ref var addComponent = ref entity.AddComponent<SampleComponent>();
    ref var getComponent = ref entity.GetOrAddComponent<SampleComponent>();
    var hasComponent = entity.HasComponent<SampleComponent>();
    entity.RemoveComponent<SampleComponent>();
}
```

Тогда можно будет работать сразу с оберткой для сущности.

Чтобы вернуться к типу `int`:
```csharp
_filter.EnumerateAsEntityId();
```

### EcsSystem

Системы конвейерно обрабатывают данные ECS мира. Это значит, что они активируются **все** и именно в той последовательности, в которой они были добавлены в `EcsWorld`.
Для вызова систем используется один из методов мира:
```csharp
world.Init();
world.Run();
world.Dispose();
```

> Системы, реализующие интерфейс `IPostRunSystem` будут вызваны в соответствующем порядке, после отработки всех `IRunSystem`. Все вызовы происходят за один вызов `world.Run()`.


## Предупреждения

> Из-за использования статического контекста, создание более чем одного ECS мира в программе может привести к непредвиденным ошибкам и сбоям.

> Фреймворк не является потокобезопасным.

